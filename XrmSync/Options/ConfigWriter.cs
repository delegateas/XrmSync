using Microsoft.Extensions.Logging;
using System.Text.Json;
using XrmSync.Model;

namespace XrmSync.Options;

public interface IConfigWriter
{
    Task SaveConfigAsync(XrmSyncOptions options, string? filePath = null, CancellationToken cancellationToken = default);
    Task SaveAnalysisConfigAsync(PluginAnalysisOptions options, string? filePath = null, CancellationToken cancellationToken = default);
}

public class ConfigWriter(ILogger<ConfigWriter> logger) : IConfigWriter
{
    public async Task SaveConfigAsync(XrmSyncOptions options, string? filePath = null, CancellationToken cancellationToken = default)
    {
        var targetFile = filePath ?? $"{ConfigReader.CONFIG_FILE_BASE}.json";
        
        logger.LogInformation("Saving sync configuration to {FilePath}", targetFile);

        var config = new
        {
            XrmSync = new
            {
                Plugin = new
                {
                    Sync = new
                    {
                        AssemblyPath = options.AssemblyPath,
                        SolutionName = options.SolutionName,
                        DryRun = options.DryRun,
                        LogLevel = options.LogLevel.ToString()
                    }
                }
            }
        };

        await SaveConfigInternal(config, targetFile, cancellationToken);
    }

    public async Task SaveAnalysisConfigAsync(PluginAnalysisOptions options, string? filePath = null, CancellationToken cancellationToken = default)
    {
        var targetFile = filePath ?? $"{ConfigReader.CONFIG_FILE_BASE}.json";
        
        logger.LogInformation("Saving analysis configuration to {FilePath}", targetFile);

        var config = new
        {
            XrmSync = new
            {
                Plugin = new
                {
                    Analysis = new
                    {
                        AssemblyPath = options.AssemblyPath,
                        PublisherPrefix = options.PublisherPrefix,
                        PrettyPrint = options.PrettyPrint
                    }
                }
            }
        };

        await SaveConfigInternal(config, targetFile, cancellationToken);
    }

    private async Task SaveConfigInternal(object config, string targetFile, CancellationToken cancellationToken)
    {
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
                var existingConfig = JsonSerializer.Deserialize<JsonElement>(existingContent);
                
                if (existingConfig.ValueKind == JsonValueKind.Object)
                {
                    var mergedConfig = MergeConfigurations(existingConfig, config, jsonOptions);
                    var mergedJson = JsonSerializer.Serialize(mergedConfig, jsonOptions);
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

    private Dictionary<string, object> MergeConfigurations(JsonElement existing, object newConfig, JsonSerializerOptions jsonOptions)
    {
        var result = new Dictionary<string, object>();
        
        // Copy existing properties
        foreach (var property in existing.EnumerateObject())
        {
            result[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText()) ?? new object();
        }

        // Serialize the new config to get its structure
        var newConfigJson = JsonSerializer.Serialize(newConfig, jsonOptions);
        var newConfigElement = JsonSerializer.Deserialize<JsonElement>(newConfigJson);

        // Merge the new config
        foreach (var property in newConfigElement.EnumerateObject())
        {
            if (property.Name == "XrmSync" && result.ContainsKey("XrmSync"))
            {
                // Deep merge XrmSync section
                var existingXrmSync = result["XrmSync"];
                var newXrmSync = JsonSerializer.Deserialize<Dictionary<string, object>>(property.Value.GetRawText());
                result["XrmSync"] = MergeXrmSyncSection(existingXrmSync, newXrmSync ?? new Dictionary<string, object>());
            }
            else
            {
                result[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText()) ?? new object();
            }
        }

        return result;
    }

    private object MergeXrmSyncSection(object existing, Dictionary<string, object> newSection)
    {
        if (existing is JsonElement existingElement && existingElement.ValueKind == JsonValueKind.Object)
        {
            var result = new Dictionary<string, object>();
            
            // Copy existing properties
            foreach (var property in existingElement.EnumerateObject())
            {
                result[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText()) ?? new object();
            }

            // Merge new properties
            foreach (var kvp in newSection)
            {
                if (kvp.Key == "Plugin" && result.ContainsKey("Plugin"))
                {
                    // Deep merge Plugin section
                    var existingPlugin = result["Plugin"];
                    var newPlugin = kvp.Value;
                    result["Plugin"] = MergePluginSection(existingPlugin, newPlugin);
                }
                else
                {
                    result[kvp.Key] = kvp.Value;
                }
            }

            return result;
        }

        return newSection;
    }

    private object MergePluginSection(object existing, object newSection)
    {
        if (existing is JsonElement existingElement && existingElement.ValueKind == JsonValueKind.Object &&
            newSection is Dictionary<string, object> newDict)
        {
            var result = new Dictionary<string, object>();
            
            // Copy existing properties
            foreach (var property in existingElement.EnumerateObject())
            {
                result[property.Name] = JsonSerializer.Deserialize<object>(property.Value.GetRawText()) ?? new object();
            }

            // Merge new properties (Sync or Analysis)
            foreach (var kvp in newDict)
            {
                result[kvp.Key] = kvp.Value;
            }

            return result;
        }

        return newSection;
    }
}