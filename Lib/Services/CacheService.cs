using System.Text.Json;
using Lib.Interfaces;

namespace Lib.Services;

public class CacheService : ICacheService
{
    private readonly string _cacheDir;
    private readonly IHttpClient _client;
    private readonly IFileService _fileService;
    private readonly ILogger _logger;

    public CacheService(string cacheDir, IFileService fileService, IHttpClient client, ILogger logger)
    {
        _cacheDir = cacheDir;
        _fileService = fileService;
        _client = client;
        _logger = logger;
        fileService.CreateDirectory(_cacheDir);
    }

    public async Task Set<T>(string key, T value, CancellationToken token = default)
    {
        string cacheFilePath = Path.Combine(_cacheDir, Helper.GenerateHash(key));
        if (File.Exists(cacheFilePath)) return;

        await WriteToFile(cacheFilePath, value, token, true);
    }


    public async Task<T?> Get<T>(string key, CancellationToken token = default) where T : class
    {
        string cacheFilePath = Path.Combine(_cacheDir, Helper.GenerateHash(key));
        if (!_fileService.Exists(cacheFilePath)) return null;

        await using FileStream stream = File.OpenRead(cacheFilePath);
        return await JsonSerializer.DeserializeAsync<T>(stream, cancellationToken: token);
    }

    public async Task<string?> GetText(string url, CancellationToken token = default)
    {
        string cacheFilePath = Path.Combine(_cacheDir, Helper.GenerateHash(url) + ".txt");
        if (_fileService.Exists(cacheFilePath)) return await _fileService.ReadAllText(cacheFilePath, token);

        string? text = await _client.GetString(url, token);
        if (text is null) return null;

        await WriteToFile(cacheFilePath, text, token);
        return text;
    }

    public async Task<T?> GetJson<T>(string url, CancellationToken token = default) where T : class
    {
        string cacheFilePath = Path.Combine(_cacheDir, Helper.GenerateHash(url) + ".json");
        if (_fileService.Exists(cacheFilePath))
            return JsonSerializer.Deserialize<T>(await _fileService.ReadAllText(cacheFilePath, token));

        T? response = await _client.GetJson<T>(url, token);
        if (response == null) return null;

        await WriteToFile(cacheFilePath, JsonSerializer.Serialize(response), token);
        return response;
    }


    public async Task<bool> Download(
        List<(string fileName, bool isCorrect)> possibleFileNames,
        string destinationPath,
        IReadOnlyDictionary<string, WebScraper.FileState> localFileHashIndex,
        IReadOnlyDictionary<string, WebScraper.FileState> secondaryCopyHashIndex,
        string downloadUrl,
        long downloadFileSize,
        CancellationToken token = default)
    {
        Func<long?, string?, Task<bool>> downloadHelper = DownloadHelper(downloadUrl, destinationPath, token);


        foreach ((string fileName, bool isCorrect) item in possibleFileNames)
        {
            WebScraper.FileState? localFilePath = localFileHashIndex.GetValueOrDefault(item.fileName);

            if (localFilePath is null) continue;

            return item.isCorrect switch
            {
                // TODO: DOWNLOAD
                true when downloadFileSize == localFilePath.Size => true,
                true when downloadFileSize != localFilePath.Size => await downloadHelper(localFilePath.Size, null),
                // TODO: COPY OR COPY AND DOWNLOAD
                _ => await CopyAndDownloadHelper(downloadHelper, destinationPath, localFilePath, downloadFileSize)
            };
        }

        IEnumerable<WebScraper.FileState> backupCopies = possibleFileNames
            .Select(item => secondaryCopyHashIndex.GetValueOrDefault(item.fileName))
            .OfType<WebScraper.FileState>().Distinct();
        foreach (WebScraper.FileState copyFilePath in backupCopies)
            // TODO: COPY OR COPY AND DOWNLOAD
            return await CopyAndDownloadHelper(downloadHelper, destinationPath, copyFilePath, downloadFileSize);

        // TODO: DOWNLOAD
        return await downloadHelper(null, null);
    }

    private static async Task<bool> CopyAndDownloadHelper(Func<long?, string?, Task<bool>> downloadHelper,
        string destinationPath, WebScraper.FileState target, long downloadFileSize)
    {
        if (downloadFileSize == target.Size) return await downloadHelper(null, target.Path);

        bool result = await downloadHelper(null, target.Path);
        if (!result) return false;
        return await downloadHelper(target.Size, null);
    }

    private Func<long?, string?, Task<bool>> DownloadHelper(string downloadUrl, string destinationPath,
        CancellationToken token = default)
    {
        return async (start, sourcePath) =>
        {
            await using Stream? sourceStream = sourcePath is not null
                ? _fileService.Open(sourcePath, start)
                : _client.DownloadStream(downloadUrl, token, start);

            if (sourceStream is null) return false;

            await using FileStream destinationStream = _fileService.CreateOrOpen(destinationPath, start);
            await sourceStream.CopyToAsync(destinationStream, token);

            return true;
        };
    }


    private async Task WriteToFile<T>(string path, T text, CancellationToken token, bool isJson = false)
    {
        try
        {
            if (!isJson)
            {
                await _fileService.WriteAllText(path, text?.ToString() ?? "", token);
                return;
            }

            await using FileStream stream = new(path, FileMode.Create);
            await JsonSerializer.SerializeAsync(stream, text, cancellationToken: token);
        }
        catch (IOException)
        {
        }
        catch (Exception exception)
        {
            _logger.Log(ILogger.Level.Error, $"Exception: {exception.Message}");
        }
    }
}
