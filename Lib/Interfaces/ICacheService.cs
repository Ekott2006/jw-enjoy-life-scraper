using Lib.Services;

namespace Lib.Interfaces;

public interface ICacheService
{
    public Task Set<T>(string key, T value, CancellationToken token = default);
    public Task<T?> Get<T>(string key, CancellationToken token = default) where T : class;
    public Task<string?> GetText(string url, CancellationToken token = default);
    public Task<T?> GetJson<T>(string url, CancellationToken token = default) where T : class;

    public Task<bool> Download(
        List<(string fileName, bool isCorrect)> possibleFileNames,
        string destinationPath,
        IReadOnlyDictionary<string, WebScraper.FileState> localFileHashIndex,
        IReadOnlyDictionary<string, WebScraper.FileState> secondaryCopyHashIndex,
        string downloadUrl,
        long downloadFileSize,
        CancellationToken token = default
    );
}
