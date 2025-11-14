using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Serialization;
using XrmSync.Model;
using XrmSync.Model.Exceptions;

namespace XrmSync.Options;

public interface IConfigWriter
{
    Task SaveConfig(string? filePath = null, CancellationToken cancellationToken = default);
}

internal class ConfigWriter(IOptions<XrmSyncConfiguration> options, ILogger<ConfigWriter> logger) : IConfigWriter
{
    public async Task SaveConfig(string? filePath = null, CancellationToken cancellationToken = default)
    {
        var targetFile = filePath ?? $"{ConfigReader.CONFIG_FILE_BASE}.json";

        logger.LogInformation("Saving configuration to {FilePath}", targetFile);

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

            // Serialize the configuration directly to XrmSync section
            var configJson = JsonSerializer.Serialize(options.Value, jsonOptions);
            rootConfig[XrmSyncConfigurationBuilder.SectionName.XrmSync] = JsonSerializer.Deserialize<Dictionary<string, object?>>(configJson, jsonOptions);

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