using Microsoft.Extensions.Configuration;

namespace XrmSync.Options;

public interface IConfigReader
{
    IConfiguration GetConfiguration();
    string? ResolveConfigurationName(string? requestedName);
}

internal class ConfigReader : IConfigReader
{
    public const string CONFIG_FILE_BASE = "appsettings";
    private const string DEFAULT_CONFIG_NAME = "default";

    public IConfiguration GetConfiguration()
    {
        return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"{CONFIG_FILE_BASE}.json", optional: true)
                .AddJsonFile($"{CONFIG_FILE_BASE}.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
    }

    public string? ResolveConfigurationName(string? requestedName)
    {
        var configuration = GetConfiguration();
        var xrmSyncSection = configuration.GetSection("XrmSync");
        
        if (!xrmSyncSection.Exists())
        {
            return null;
        }

        // Get all configuration names (direct children of XrmSync)
        var configNames = xrmSyncSection.GetChildren()
            .Select(c => c.Key)
            .Where(k => k != "Plugin") // Exclude legacy structure
            .ToList();

        // If requested name is specified, use it if it exists
        if (!string.IsNullOrWhiteSpace(requestedName))
        {
            return configNames.Contains(requestedName) ? requestedName : null;
        }

        // If only one named config exists, use it
        if (configNames.Count == 1)
        {
            return configNames[0];
        }

        // If multiple configs exist, try to use "default"
        if (configNames.Contains(DEFAULT_CONFIG_NAME))
        {
            return DEFAULT_CONFIG_NAME;
        }

        // Fall back to legacy structure if no named configs exist
        if (configNames.Count == 0 && xrmSyncSection.GetSection("Plugin").Exists())
        {
            return null; // Use legacy structure
        }

        return null;
    }
}
