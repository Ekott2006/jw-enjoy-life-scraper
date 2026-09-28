namespace Lib.Interfaces;

public interface IHttpClient
{
    Task<string?> GetString(string url, CancellationToken cancellationToken);
    Task<T?> GetJson<T>(string url, CancellationToken cancellationToken);

    public Stream? DownloadStream(string url, CancellationToken cancellationToken, long? start = null,
        long? end = null);
}
