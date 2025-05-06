using System.Text.Json;
using Lib.Interfaces;

namespace Lib.Services;

public class ConfigurationService(IFileService fileService, ILogger logger)
{
    public async Task<Dictionary<string, Dictionary<string, string>>?> GetConfig(string configFile,
        CancellationToken token = default)
    {
        Dictionary<string, Dictionary<string, string>>? config =
            JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(
                await fileService.ReadAllText(configFile, token));
        if (config is not null) return config;
        logger.Log(ILogger.Level.Error, "No Config File Found", ILogger.Options.Bold);
        return null;
    }
}