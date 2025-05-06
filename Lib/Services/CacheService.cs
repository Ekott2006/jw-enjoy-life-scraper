using System.Text.Json;
using Lib.Interfaces;

namespace Lib.Services;

public class CacheService : ICacheService
{
    private readonly string _cacheDir;
    private readonly IHttpClient _client;
    private readonly IFileService _fileService;

    public CacheService(string cacheDir, IFileService fileService, IHttpClient client,
        IDirectoryService directoryService)
    {
        _cacheDir = cacheDir;
        _fileService = fileService;
        _client = client;
        directoryService.CreateDirectory(_cacheDir);
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

    public async Task<bool> Download(string dir, string url, string filename, CancellationToken token = default)
    {
        await Task.Delay(50, token);
        string cacheFilePath = Path.Combine(dir, Helper.SanitizeFileName(filename));
        if (_fileService.Exists(cacheFilePath)) return true;

        Stream? downloadStream = _client.DownloadStream(url, token);
        if (downloadStream is null) return false;

        Directory.CreateDirectory(dir);
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
            Console.WriteLine("Exception: {0}", exception);
        }
    }
}