using Microsoft.Extensions.Logging;
using System.Text.Json;
using XrmSync.Model;

namespace XrmSync.Options;

public interface IConfigWriter
{
    Task SaveConfigAsync(XrmSyncOptions options, string? filePath = null, CancellationToken cancellationToken = default);
}

public class ConfigWriter(ILogger<ConfigWriter> logger) : IConfigWriter
{
    public async Task SaveConfigAsync(XrmSyncOptions options, string? filePath = null, CancellationToken cancellationToken = default)
    {
        var targetFile = filePath ?? $"{ConfigReader.CONFIG_FILE_BASE}.json";
        
        logger.LogInformation("Saving configuration to {FilePath}", targetFile);

        var config = new
        {
            XrmSync = new
            {
                AssemblyPath = options.AssemblyPath,
                SolutionName = options.SolutionName,
                DryRun = options.DryRun,
                LogLevel = options.LogLevel.ToString()
            }
        };

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = null // Keep original casing
        };

        // If file exists, try to merge with existing content
        if (File.Exists(targetFile))
        {
            try
            {
                var existingContent = await File.ReadAllTextAsync(targetFile, cancellationToken);
                var existingConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(existingContent);
                
                if (existingConfig != null)
                {
                    existingConfig["XrmSync"] = JsonSerializer.SerializeToElement(config.XrmSync, jsonOptions);
                    var mergedJson = JsonSerializer.Serialize(existingConfig, jsonOptions);
                    await File.WriteAllTextAsync(targetFile, mergedJson, cancellationToken);
                    logger.LogInformation("Configuration merged with existing file");
                    return;
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Could not merge with existing configuration file, will overwrite");
            }
        }

        // Write new file or overwrite if merge failed
        var json = JsonSerializer.Serialize(config, jsonOptions);
        await File.WriteAllTextAsync(targetFile, json, cancellationToken);
        
        logger.LogInformation("Configuration saved successfully to {FilePath}", targetFile);
    }
}