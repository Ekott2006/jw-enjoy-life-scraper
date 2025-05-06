using Lib.Interfaces;
using RestSharp;

namespace Lib.Services;

public class CustomHttpClient : IHttpClient
{
    private readonly RestClient _client = new();

    public async Task<string?> GetString(string url, CancellationToken cancellationToken)
    {
        RestRequest request = new(url);
        return (await _client.GetAsync(request, cancellationToken)).Content;
    }

    public async Task<T?> GetJson<T>(string url, CancellationToken cancellationToken)
    {
        RestRequest request = new(url);
        return await _client.GetAsync<T>(request, cancellationToken);
    }

    public Stream? DownloadStream(string url, CancellationToken cancellationToken)
    {
        RestRequest request = new(url);
        return _client.DownloadStream(request, cancellationToken);
    }
}