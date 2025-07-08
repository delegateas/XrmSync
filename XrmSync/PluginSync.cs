using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XrmSync.Model;
using XrmSync.SyncService;

namespace XrmSync;

internal static class PluginSync
{
    public static async Task RunSync(IServiceProvider services)
    {
        var options = services.GetRequiredService<XrmSyncOptions>();
        var description = services.GetRequiredService<Description>();

        var log = services.GetRequiredService<ILogger>();
        log.LogInformation("{header}", description.ToolHeader);

        if (options.DryRun)
        {
            log.LogInformation("***** DRY RUN *****");
            log.LogInformation("No changes will be made to Dataverse.");
        }

        if (options.DataverseUrl is not null)
        {
            log.LogInformation("Connecting to Dataverse at {dataverseUrl}", options.DataverseUrl);
        }

        var pluginSyncService = ActivatorUtilities.CreateInstance<PluginSyncService>(services);
        await pluginSyncService.Sync();
    }
}
