using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using XrmSync.AssemblyAnalyzer.Extensions;
using XrmSync.Dataverse.Extensions;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.SyncService;
using XrmSync.SyncService.Extensions;
using DGLoggerFactory = XrmSync.LoggerFactory;

namespace XrmSync;

internal static class PluginSync
{
    public static async Task<bool> RunSync(XrmSyncOptions options)
    {
        var services = RegisterServices(options);

        var log = services.GetRequiredService<ILogger>();
        var description = services.GetRequiredService<Description>();
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

        try
        {
            await pluginSyncService.Sync();
            return true;
        }
        catch (XrmSyncException ex)
        {
            var logger = services.GetRequiredService<ILogger>();
            logger.LogError("Error during synchronization: {message}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger>();
            logger.LogCritical(ex, "An unexpected error occurred during synchronization");
            return false;
        }
    }

    private static ServiceProvider RegisterServices(XrmSyncOptions options)
    {
        var services = new ServiceCollection();

        services.AddSingleton(options);
        services.AddSingleton((_) => DGLoggerFactory.GetLogger<ISyncService>());
        services.AddAssemblyAnalyzer();
        services.AddSyncService();
        services.AddDataverseConnection(options);

        return services.BuildServiceProvider();
    }
}
