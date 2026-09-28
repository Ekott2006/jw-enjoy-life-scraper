using System.Text.Json;
using System.Text.RegularExpressions;
using Lib.Interfaces;

namespace Lib.Services;

public partial class CacheService : ICacheService
{
    private readonly string _cacheDir;
    private readonly IHttpClient _client;
    private readonly ILogger _logger;
    private readonly IFileService _fileService;

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

    public async Task<bool> Download(string dir, string url, string filename, int fileSize,
        CancellationToken token = default)
    {
        string cacheFilePath = Path.Combine(dir, Helper.SanitizeFileName(filename));
        FileInfo fileInfo = _fileService.GetFileData(cacheFilePath);
        if (fileInfo.Exists && fileInfo.Length == fileSize) return true;

        Stream? downloadStream = _client.DownloadStream(url, token);
        if (downloadStream is null) return false;

        _fileService.CreateDirectory(dir);
        await using FileStream fileStream = _fileService.Create(cacheFilePath);
        await downloadStream.CopyToAsync(fileStream, token);
        return true;
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

    public async Task<bool> CopyIfExist(string dir, string filename, string localPath, string resolution, CancellationToken token)
    {
        string sFilename = Helper.SanitizeFileName(filename);
        string src = Path.Combine(localPath, sFilename);
        string[] exist = Directory.GetFiles(localPath, allResolutionRegex().Replace(sFilename, "*"));
        if (exist.Length == 0) { return false; }

        string? selectedFile = exist
            .Select(file =>
            {
                Match match = extractResolutionRegex().Match(file);
                return new
                {
                    File = file,
                    Resolution = match.Success ? int.Parse(match.Groups[1].Value) : 0
                };
            })
            .Where(x => x.Resolution >= int.Parse(resolution[..^1]))
            .OrderBy(x => x.Resolution)
            .FirstOrDefault()?.File;

        if (selectedFile is null) return false;
        _fileService.CreateDirectory(dir);
        File.Copy(selectedFile, Path.Combine(dir, sFilename));
        return true;
    }

    [GeneratedRegex(@"(?<=_r)\d+(?=P\.mp4$)")]
    private static partial Regex allResolutionRegex();
    [GeneratedRegex(@"_r(\d+)P\.mp4$")]
    private static partial Regex extractResolutionRegex();
}
