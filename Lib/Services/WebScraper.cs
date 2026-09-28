using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using HtmlAgilityPack;
using Lib.Dto;
using Lib.Interfaces;

namespace Lib.Services;

public class WebScraper(
    IFileService fileService,
    ICacheService cacheService,
    ILogger logger,
    ParallelOptions parallelOptions) : IWebScraper
{
    public async Task<Dictionary<string, List<string>>> ScrapeJsonUrlsFromSections(
        Dictionary<string, string> sectionUrls, Action<decimal>? callback = null)
    {
        ConcurrentDictionary<string, List<string>> jsonUrlsBySection = new();
        StrongBox<int> taskCompleted = new();
        await ProcessParallelBatchAsync(sectionUrls, taskCompleted,
            async (item, token) =>
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
            CreateProgressHandler(sectionUrls.Count, callback)
        );
        return jsonUrlsBySection.ToDictionary();
    }

    public async Task<Dictionary<string, List<JsonResponse>>> ScrapeJsonResponseFromUrls(
        Dictionary<string, List<string>> sectionUrls, Action<decimal>? callback = null)
    {
        Dictionary<string, List<JsonResponse>> jsonResponses = [];
        StrongBox<int> taskCompleted = new(0);
        int totalTasks = sectionUrls.Values.SelectMany(x => x).Count();

        foreach (KeyValuePair<string, List<string>> sectionUrl in sectionUrls)
        {
            ConcurrentBag<JsonResponse> jsonResponse = [];

            await ProcessParallelBatchAsync(sectionUrl.Value, taskCompleted,
                async (item, token) =>
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
                CreateProgressHandler(totalTasks, callback)
            );
            jsonResponses.Add(sectionUrl.Key, jsonResponse.ToList());
        }

        return jsonResponses;
    }


    public async Task DownloadAllFiles(Dictionary<string, List<JsonResponse>> sectionUrls, string resolution,
        string? copyDir, string targetDir, IWebScraper.DownloadFileNameType fileNameType,
        IWebScraper.DownloadFilePathType filePathType,
        Action<decimal>? callback = null)
    {
        fileService.CreateDirectory(targetDir);
        ConcurrentDictionary<string, FileState> localFileHashIndex =
            new(GetFileStateIndex(targetDir));
        Dictionary<string, FileState> secondaryCopyHashIndex =
            copyDir is null ? [] : GetFileStateIndex(copyDir);


        int totalTasks = sectionUrls.Values.SelectMany(x => x).Count();
        StrongBox<int> doneTask = new(0);

        foreach (KeyValuePair<string, List<JsonResponse>> item in sectionUrls)
        {
            List<MovieDetail> result =
                item.Value.Select(x =>
                        x.Files.Values.FirstOrDefault()?.MovieMp4S.FirstOrDefault(movie => movie.Label == resolution))
                    .OfType<MovieDetail>().ToList();
            await ProcessParallelBatchAsync(result, doneTask,
                async (video, token) =>
                {
                    string urlPath = new Uri(video.File.Url).AbsolutePath;
                    string dir = filePathType == IWebScraper.DownloadFilePathType.Grouped
                        ? Path.Combine(targetDir, item.Key)
                        : targetDir;
                    fileService.CreateDirectory(dir);

                    List<(string fileName, bool isCorrect)> possibleFileNames =
                    [
                        (Path.GetFileName(urlPath), fileNameType == IWebScraper.DownloadFileNameType.JwLibrary),

                        (video.Title + Path.GetExtension(urlPath),
                            fileNameType != IWebScraper.DownloadFileNameType.JwLibrary)
                    ];
                    possibleFileNames =
                    [
                        .. possibleFileNames
                            .Select(x => (Helper.SanitizeFileName(x.fileName), x.isCorrect))
                    ];
                    string fileName = possibleFileNames.Find(x => x.isCorrect).fileName;
                    string destinationPath = Path.Combine(dir, fileName);

                    bool isDownloaded = await cacheService.Download(
                        possibleFileNames,
                        destinationPath,
                        localFileHashIndex,
                        secondaryCopyHashIndex,
                        video.File.Url,
                        video.FileSize,
                        token
                    );

                    if (isDownloaded)
                        localFileHashIndex.TryAdd(fileName,
                            new FileState(destinationPath, video.FileSize));
                },
                (exception, video) =>
                {
                    logger.Log(ILogger.Level.Error,
                        $"Error downloading Videos in section {item.Key}: {exception.Message}. URL: {video.File.Url}");
                },
                CreateProgressHandler(totalTasks, callback)
            );
        }
    }

    private Dictionary<string, FileState> GetFileStateIndex(string path)
    {
        return fileService.GetAllFilePaths(path)
            .Select(filePath => (Name: Helper.SanitizeFileName(Path.GetFileName(filePath)), Path: filePath,
                Size: fileService.GetFileData(filePath).Length))
            .DistinctBy(item => item.Name)
            .ToDictionary(item => item.Name, item => new FileState(item.Path, item.Size));
    }

    private async Task<List<string>> ScrapeHtml(string html, CancellationToken token = default)
    {
        List<string>? cache = await cacheService.Get<List<string>>(html, token);
        if (cache is not null) return cache;

        const string nodeXPath = "//div[contains(@class, 'du-margin-children-vertical-desktopOnly--0')]";
        HtmlDocument htmlDoc = new();
        htmlDoc.LoadHtml(html);
        List<HtmlNode> nodeList = htmlDoc.DocumentNode.SelectNodes(nodeXPath)?.ToList() ?? [];
        List<string> result =
        [
            .. nodeList
                .Select(x => x.SelectSingleNode(".//div")?.Attributes["data-jsonurl"].Value)
                .OfType<string>()
        ];
        await cacheService.Set(html, result, token);
        return result;
    }

    private async Task ProcessParallelBatchAsync<T>(
        IEnumerable<T> items,
        StrongBox<int> taskCompleted,
        Func<T, CancellationToken, ValueTask>? bodyAction = null,
        Action<Exception, T>? errorAction = null,
        Action<int>? progressAction = null)
    {
        Lock progressLock = new();

        await Parallel.ForEachAsync(items, parallelOptions, async (item, token) =>
        {
            try
            {
                if (bodyAction is not null) await bodyAction(item, token);
            }
            catch (Exception exception)
            {
                errorAction?.Invoke(exception, item);
            }
            finally
            {
                Interlocked.Increment(ref taskCompleted.Value);
                lock (progressLock)
                {
                    progressAction?.Invoke(taskCompleted.Value);
                }
            }
        });
    }

    private static Action<int> CreateProgressHandler(int total, Action<decimal>? callback = null)
    {
        return progress => callback?.Invoke(Helper.ToPercentage(progress, total));
    }

    public record FileState(string Path, long Size);
}
