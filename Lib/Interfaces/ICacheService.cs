namespace Lib.Interfaces;

public interface ICacheService
{
    public Task Set<T>(string key, T value, CancellationToken token = default);
    public Task<T?> Get<T>(string key, CancellationToken token = default) where T : class;
    public Task<string?> GetText(string url, CancellationToken token = default);
    public Task<T?> GetJson<T>(string url, CancellationToken token = default) where T : class;
    public Task<bool> Download(string dir, string url, string filename, CancellationToken token = default);
}