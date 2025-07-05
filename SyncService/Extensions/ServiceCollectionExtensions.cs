using DG.XrmSync.SyncService.AssemblyReader;
using DG.XrmSync.SyncService.Comparers;
using Microsoft.Extensions.DependencyInjection;
using DG.XrmSync.Model.Plugin;
using DG.XrmSync.Model.CustomApi;
using DG.XrmSync.SyncService.Differences;

namespace DG.XrmSync.SyncService.Extensions;

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
