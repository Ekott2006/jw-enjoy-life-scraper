using System.Collections.Concurrent;
using System.Runtime.Serialization;
using HtmlAgilityPack;
using Lib.Dto;
using Lib.Interfaces;

namespace Lib.Services;

public class WebScraper(ICacheService cacheService, ILogger logger, ParallelOptions parallelOptions)
{
    private async Task<List<string>> ScrapeHtml(string html, CancellationToken token = default)
    {
        List<string>? cache = await cacheService.Get<List<string>>(html, token);
        if (cache is not null) return cache;

        const string nodeXPath = "//div[contains(@class, 'du-margin-children-vertical-desktopOnly--0')]";
        HtmlDocument htmlDoc = new();
        htmlDoc.LoadHtml(html);
        List<HtmlNode> nodeList = htmlDoc.DocumentNode.SelectNodes(nodeXPath)?.ToList() ?? [];
        List<string> result = nodeList
            .Select(x => x.SelectSingleNode(".//div")?.Attributes["data-jsonurl"].Value)
            .OfType<string>()
            .ToList();
        await cacheService.Set(html, result, token);
        return result;
    }

    private async Task ParallelHelper<T>(IEnumerable<T> data, int doneTask, int allTasks,
        Func<T, CancellationToken, ValueTask>? bodyAction = null,
        Action<Exception, T>? errorAction = null, Action<decimal>? progressAction = null)
    {
        Lock @lock = new();
        await Parallel.ForEachAsync(data, parallelOptions, async (item, token) =>
        {
            try
            {
                if (bodyAction != null) await bodyAction(item, token);
            }
            catch (Exception exception)
            {
                errorAction?.Invoke(exception, item);
            }
            finally
            {
                Interlocked.Increment(ref doneTask);
                lock (@lock)
                {
                    progressAction?.Invoke((decimal)doneTask / allTasks * 100);
                }
            }
        });
    }

    public async Task<Dictionary<string, List<string>>> ScrapeJsonUrlsFromSections(
        Dictionary<string, string> sectionUrls, Action<decimal>? callback = null)
    {
        ConcurrentDictionary<string, List<string>> jsonUrlsBySection = new();
        await ParallelHelper(sectionUrls, 0, sectionUrls.Count, async (item, token) =>
            {
                string? response = await cacheService.GetText(item.Value, token);
                if (response is null) return;
                jsonUrlsBySection.TryAdd(item.Key, await ScrapeHtml(response, token));
            },
            (exception, item) =>
            {
                logger.Log(ILogger.Level.Error,
                    $"Error scraping HTML in section {item.Key}: {exception.Message}. URL: {item.Value}");
            },
            progress => { callback?.Invoke(progress); });
        return jsonUrlsBySection.ToDictionary();
    }

    public async Task<Dictionary<string, List<JsonResponse>>> ScrapeJsonResponseFromUrls(
        Dictionary<string, List<string>> sectionUrls, Action<decimal>? callback = null)
    {
        Dictionary<string, List<JsonResponse>> jsonResponses = [];
        int allTasks = sectionUrls.Values.SelectMany(x => x).Count();
        int doneTask = 0;

        foreach (KeyValuePair<string, List<string>> sectionUrl in sectionUrls)
        {
            ConcurrentBag<JsonResponse> jsonResponse = [];

            await ParallelHelper(sectionUrl.Value, doneTask, allTasks, async (item, token) =>
                {
                    JsonResponse? jsonData = await cacheService.GetJson<JsonResponse>(item, token);
                    if (jsonData is null) return;
                    jsonResponse.Add(jsonData);
                },
                (exception, item) =>
                {
                    logger.Log(ILogger.Level.Error,
                        $"Error scraping JSON in section {sectionUrl.Key}: {exception.Message}. URL: {item}");
                },
                progress => { callback?.Invoke(progress); });
            jsonResponses.Add(sectionUrl.Key, jsonResponse.ToList());
            doneTask += sectionUrl.Value.Count;
        }

        return jsonResponses;
    }


    public async Task DownloadAllFiles(Dictionary<string, List<JsonResponse>> sectionUrls, string resolution,
        string targetDir, DownloadFileNameType fileNameType, DownloadFilePathType filePathType,
        Action<decimal>? callback = null)
    {
        int allTasks = sectionUrls.Values.SelectMany(x => x).Count();
        int doneTask = 0;

        foreach (KeyValuePair<string, List<JsonResponse>> item in sectionUrls)
        {
            List<MovieDetail> result =
                item.Value.Select(x =>
                        x.Files.Values.FirstOrDefault()?.MovieMp4s.FirstOrDefault(movie => movie.Label == resolution))
                    .OfType<MovieDetail>().ToList();
            await ParallelHelper(result, doneTask, allTasks, async (video, token) =>
                {
                    string urlPath = new Uri(video.File.Url).AbsolutePath;
                    string fileName = fileNameType == DownloadFileNameType.JwLibrary
                        ? Path.GetFileName(urlPath)
                        : video.Title + Path.GetExtension(urlPath);
                    string dir = filePathType == DownloadFilePathType.Grouped
                        ? Path.Combine(targetDir, item.Key)
                        : targetDir;
                    await cacheService.Download(dir, video.File.Url, fileName, token);
                },
                (exception, video) =>
                {
                    logger.Log(ILogger.Level.Error,
                        $"Error downloading Videos in section {item.Key}: {exception.Message}. URL: {video.File.Url}");
                },
                progress => { callback?.Invoke(progress); });
            doneTask += result.Count;
        }
    }

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
}