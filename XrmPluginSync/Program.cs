using DG.XrmPluginSync;
using DG.XrmPluginSync.Dataverse.Extensions;
using DG.XrmPluginSync.SyncService.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, services) =>
    {
        services.AddSyncService();
        services.AddDataverse();
        services.AddSingleton((_) => LoggerFactory.GetLogger<PluginSync>());
        services.AddSingleton<DG.XrmPluginSync.SyncService.Common.Description>();
        services.AddTransient<DG.XrmPluginSync.SyncService.Models.Requests.SyncRequest>();
    })
    .Build();

return PluginSync.RunCliAsync(args, host).Result;
