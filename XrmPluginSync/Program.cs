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
    })
    .Build();

var program = ActivatorUtilities.CreateInstance<PluginSync>(host.Services);
program.Run(args);
