using DG.XrmPluginSync.Model;
using DG.XrmPluginSync.SyncService.Models.Requests;
using Microsoft.Extensions.DependencyInjection;

namespace DG.XrmPluginSync;

internal class PluginSync(SyncService.SyncService syncService)
{
    public void Run(SyncRequest req)
    {
        syncService.SyncPlugins(req).Wait();
    }

    public static void RunCli(IServiceProvider services)
    {
        var options = services.GetRequiredService<XrmPluginSyncOptions>();
        var req = ActivatorUtilities.CreateInstance<SyncRequest>(services);
        req.AssemblyPath = options.AssemblyPath;
        req.SolutionName = options.SolutionName;
        req.DryRun = options.DryRun;

        var program = ActivatorUtilities.CreateInstance<PluginSync>(services);
        program.Run(req);
    }
}
