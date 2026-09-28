using Lib.Dto;

namespace Lib.Interfaces;

public interface IWebScraper
{
    public enum DownloadFileNameType
    {
        JwLibrary,
        Simple
    }

    public enum DownloadFilePathType
    {
        Simple,
        Grouped
    }


    public Task<Dictionary<string, List<string>>> ScrapeJsonUrlsFromSections(
        Dictionary<string, string> sectionUrls, Action<decimal>? callback = null);

    public Task<Dictionary<string, List<JsonResponse>>> ScrapeJsonResponseFromUrls(
        Dictionary<string, List<string>> sectionUrls, Action<decimal>? callback = null);

    public Task DownloadAllFiles(Dictionary<string, List<JsonResponse>> sectionUrls, string resolution,
        string? copyDir, string targetDir, DownloadFileNameType fileNameType,
        DownloadFilePathType filePathType,
        Action<decimal>? callback = null);
}
