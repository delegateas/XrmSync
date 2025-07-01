using DG.XrmPluginSync.SyncService.AssemblyReader;
using DG.XrmPluginSync.SyncService.Comparers;
using DG.XrmPluginSync.Model;
using Microsoft.Extensions.DependencyInjection;

namespace DG.XrmPluginSync.SyncService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSyncService(this IServiceCollection services)
    {
        return services.AddSingleton<SyncService>()
            .AddSingleton<IAssemblyReader, AssemblyReader.AssemblyReader>()
            .AddSingleton<Plugin>()
            .AddSingleton<IEqualityComparer<PluginTypeEntity>, PluginTypeComparer>()
            .AddSingleton<IEqualityComparer<PluginStepEntity>, PluginStepComparer>()
            .AddSingleton<IEqualityComparer<PluginImageEntity>, PluginImageComparer>();
    }
}
