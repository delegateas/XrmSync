using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Options;
using XrmSync.SyncService;

namespace XrmSync.Actions;

internal class PluginSyncAction(PluginSyncService pluginSync, ILogger<PluginSyncAction> log) : IAction
{
    public async Task<bool> RunAction(CancellationToken cancellationToken)
    {
        try
        {
            await pluginSync.Sync(cancellationToken);
            return true;
        }
        catch (Model.Exceptions.OptionsValidationException ex)
        {
            log.LogCritical("Configuration validation failed:{nl}{message}", Environment.NewLine, ex.Message);
            return false;
        }
        catch (XrmSyncException ex)
        {
            log.LogError("Error during synchronization: {message}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            log.LogCritical(ex, "An unexpected error occurred during synchronization: {message}", ex.Message);
            return false;
        }
    }
}

internal class SavePluginSyncConfigAction(IOptions<XrmSyncConfiguration> config, IConfigWriter configWriter) : ISaveConfigAction
{
    public async Task<bool> SaveConfigAsync(string? filename, CancellationToken cancellationToken)
    {
        // Handle save-config functionality
        if (config.Value.Plugin?.Sync is null)
        {
            throw new XrmSyncException(filename is null
                ? "No sync configuration loaded - cannot save"
                : $"No sync configuration loaded - cannot save to {filename}");
        }

        var configPath = string.IsNullOrWhiteSpace(filename) ? null : filename;
        await configWriter.SavePluginSyncConfigAsync(config.Value.Plugin.Sync, configPath, cancellationToken);
        Console.WriteLine($"Configuration saved to {configPath ?? $"{ConfigReader.CONFIG_FILE_BASE}.json"}");
        return true;
    }
}
