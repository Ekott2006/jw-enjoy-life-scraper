using Console;
using Lib;
using Lib.Dto;
using Lib.Interfaces;
using Lib.Services;

const string cacheDir = ".cache";
const string configFile = "config.json";
const string downloadDir = "files";

ParallelOptions parallelOptions = new() { MaxDegreeOfParallelism = 8 };
ConsoleLogger logger = new();
CustomFile file = new();
CustomHttpClient httpClient = new();
CustomDirectory directory = new();
ConfigurationService configuration = new(file, logger);
CacheService cacheService = new(cacheDir, file, httpClient, directory);
WebScraper webScraper = new(cacheService, logger, parallelOptions);

logger.Log(ILogger.Level.Figlet, "JW Scraper");

// Read the **config.json** (get the section links)
logger.Log(ILogger.Level.Info,
    "Welcome to the Enjoy life Scrapper. A CLI that scrapes JW.ORG and gets enjoy life videos to download in bulk",
    ILogger.Options.Bold);

Dictionary<string, Dictionary<string, string>>? config = await configuration.GetConfig(configFile);
if (config is null) return;
logger.Log(ILogger.Level.Info, "Config Found");

// Ask the user to select the Language and get from config
string language = logger.Prompt("Select The Language:", config.Keys);
Dictionary<string, string> sectionUrls = config[language];

// Make the first requests to get the **html** for each section
logger.Log(ILogger.Level.Info, "Starting to Scrape the HTML", ILogger.Options.Bold);
Dictionary<string, List<string>> jsonUrls = await logger.WithProgress("HTML Progress:",
    async action => await webScraper.ScrapeJsonUrlsFromSections(sectionUrls, action));

// Make the second batch of request to get all the API JSON Links
logger.Log(ILogger.Level.Info, "Starting to Scrape the JSON", ILogger.Options.Bold);
Dictionary<string, List<JsonResponse>> jsonResponseFromUrls = await logger.WithProgress("JSON Progress:",
    async action => await webScraper.ScrapeJsonResponseFromUrls(jsonUrls, action));

// Some Metadata by getting the size by resolution, (3gp, 240, 360, 480, 720),
Dictionary<string, decimal> metadata = Helper.GenerateMetadata(jsonResponseFromUrls.Values.SelectMany(x => x));

// Ask the user what resolution and folder directory, if they want to use understandable or jwlibrary names, if they want grouping
string resolution = GenerateResolution();
string targetDir = GenerateTargetDirectory();
WebScraper.DownloadFileNameType downloadFileNameType = GenerateDownloadFileNameType();
WebScraper.DownloadFilePathType downloadFilePathType = GenerateDownloadFilePathType();

// Write or stream to the files
System.Console.WriteLine("Starting to Download the Files");
await logger.WithProgress("Download Progress:", async action =>
{
    await webScraper.DownloadAllFiles(jsonResponseFromUrls, resolution, targetDir, downloadFileNameType,
        downloadFilePathType, action);
    return Task.CurrentId;
});
return;

string GenerateResolution()
{
    Dictionary<string, string> dictionary = [];
    foreach (KeyValuePair<string, decimal> pair in metadata)
    {
        dictionary.Add(pair.Key, $"{pair.Key}: {Helper.FormatFileSize(pair.Value)}");
    }
    string prompt = logger.Prompt("Select The Resolution:", dictionary.Values);
    return dictionary
        .First(pair => pair.Value.Equals(prompt, StringComparison.OrdinalIgnoreCase)).Key;
}
string GenerateTargetDirectory()
{
    string targetDirPrompt =
        logger.Prompt("What is the directory to store the files(empty is the current directory)?", true);
    return Directory.Exists(targetDirPrompt)
        ? targetDirPrompt
        : Path.Combine(Directory.GetCurrentDirectory(), downloadDir);
}
WebScraper.DownloadFileNameType GenerateDownloadFileNameType()
{
    Dictionary<WebScraper.DownloadFileNameType, string> downloadFileNameHelper = new()
    {
        { WebScraper.DownloadFileNameType.JwLibrary, "JW Library Style (Difficult to Read File Name)" },
        { WebScraper.DownloadFileNameType.Simple, "Simple (Easy to Read File Name)" },
    };

    string downloadFileNameTypePrompt =
        logger.Prompt("Select The File Name Type:", downloadFileNameHelper.Values.Select(x => x));
    return downloadFileNameHelper
        .First(pair => pair.Value.Equals(downloadFileNameTypePrompt, StringComparison.OrdinalIgnoreCase)).Key;
}

WebScraper.DownloadFilePathType GenerateDownloadFilePathType()
{
    Dictionary<WebScraper.DownloadFilePathType, string> downloadFilePathHelper = new()
    {
        { WebScraper.DownloadFilePathType.Grouped, "JW Library Style (Videos divided by Section [config])" },
        { WebScraper.DownloadFilePathType.Simple, "Simple (All Videos in one Directory)" },
    };

    string downloadFilePathTypePrompt =
        logger.Prompt("Select The File Path Type:", downloadFilePathHelper.Values.Select(x => x));
    return downloadFilePathHelper
        .First(pair => pair.Value.Equals(downloadFilePathTypePrompt, StringComparison.OrdinalIgnoreCase)).Key;
}