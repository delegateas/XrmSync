using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using XrmSync.Model;
using XrmSync.Model.Exceptions;

namespace XrmSync.Options;

public interface IConfigWriter
{
    Task SaveConfig(string? filePath = null, string configName = XrmSyncConfigurationBuilder.DEFAULT_CONFIG_NAME, CancellationToken cancellationToken = default);
}

internal class ConfigWriter(IOptions<XrmSyncConfiguration> options, ILogger<ConfigWriter> logger) : IConfigWriter
{
    public async Task SaveConfig(string? filePath = null, string configName = XrmSyncConfigurationBuilder.DEFAULT_CONFIG_NAME, CancellationToken cancellationToken = default)
    {
        var targetFile = filePath ?? $"{ConfigReader.CONFIG_FILE_BASE}.json";

        logger.LogInformation("Saving configuration to {FilePath} with config name {ConfigName}", targetFile, configName);

        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = null, // Keep original casing
            Converters = { new JsonStringEnumConverter() } // Serialize enums as strings
        };

        try
        {
            Dictionary<string, object?> rootConfig;

            // Load existing file if it exists
            if (File.Exists(targetFile))
            {
                var existingContent = await File.ReadAllTextAsync(targetFile, cancellationToken);
                rootConfig = JsonSerializer.Deserialize<Dictionary<string, object?>>(existingContent, jsonOptions) 
                    ?? [];
            }
            else
            {
                rootConfig = [];
            }

            // Get or create XrmSync section
            if (!rootConfig.TryGetValue(XrmSyncConfigurationBuilder.SectionName.XrmSync, out var xrmSyncObj))
            {
                xrmSyncObj = new Dictionary<string, object?>();
                rootConfig[XrmSyncConfigurationBuilder.SectionName.XrmSync] = xrmSyncObj;
            }

            // Deserialize the XrmSync section to work with it
            var xrmSyncJson = JsonSerializer.Serialize(xrmSyncObj, jsonOptions);
            var xrmSyncSection = JsonSerializer.Deserialize<Dictionary<string, object?>>(xrmSyncJson, jsonOptions)
                ?? [];

            // Serialize the configuration to a structure we can work with
            var configJson = JsonSerializer.Serialize(options.Value, jsonOptions);

            // Save to appropriate location based on config name
            // Named structure: save under XrmSync.{configName}
            xrmSyncSection[configName] = JsonSerializer.Deserialize<Dictionary<string, object?>>(configJson, jsonOptions);

            rootConfig[XrmSyncConfigurationBuilder.SectionName.XrmSync] = xrmSyncSection;

            // Serialize and save
            var json = JsonSerializer.Serialize(rootConfig, jsonOptions);
            await File.WriteAllTextAsync(targetFile, json, cancellationToken);

            logger.LogInformation("Configuration saved successfully to {FilePath}", targetFile);
        }
        catch (Exception ex)
        {
            throw new XrmSyncException("Failed to save configuration", ex);
        }
    }
}