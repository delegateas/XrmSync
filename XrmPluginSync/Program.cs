using DG.XrmPluginSync;
using DG.XrmPluginSync.Dataverse.Extensions;
using DG.XrmPluginSync.SyncService.Extensions;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSyncService();
        services.AddDataverse();
    })
    .Build();

return PluginSync.RunCliAsync(args, host).Result;
