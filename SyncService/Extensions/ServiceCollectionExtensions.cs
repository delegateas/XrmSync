using DG.XrmPluginSync.SyncService.AssemblyReader;
using DG.XrmPluginSync.SyncService.Comparers;
using Microsoft.Extensions.DependencyInjection;
using DG.XrmPluginSync.Model.Plugin;
using DG.XrmPluginSync.SyncService.Common;
using DG.XrmPluginSync.Model.CustomApi;

namespace DG.XrmPluginSync.SyncService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSyncService(this IServiceCollection services)
    {
        return services
            .AddSingleton<IAssemblyReader, AssemblyReader.AssemblyReader>()
            .AddSingleton<Description>()
            .AddSingleton<PluginSyncService>()
            .AddSingleton<IDifferenceUtility, DifferenceUtility>()
            .AddSingleton<IEntityComparer<PluginType>, PluginTypeComparer>()
            .AddSingleton<IEntityComparer<Step>, PluginStepComparer>()
            .AddSingleton<IEntityComparer<Image>, PluginImageComparer>()
            .AddSingleton<IEntityComparer<ApiDefinition>, CustomApiComparer>()
            .AddSingleton<IEntityComparer<RequestParameter>, RequestParameterComparer>()
            .AddSingleton<IEntityComparer<ResponseProperty>, ResponsePropertyComparer>();
    }
}
