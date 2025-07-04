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
            .AddSingleton<IEqualityComparer<PluginType>, PluginTypeComparer>()
            .AddSingleton<IEqualityComparer<Step>, PluginStepComparer>()
            .AddSingleton<IEqualityComparer<Image>, PluginImageComparer>()
            .AddSingleton<IEqualityComparer<ApiDefinition>, CustomApiComparer>()
            .AddSingleton<IEqualityComparer<RequestParameter>, RequestParameterComparer>()
            .AddSingleton<IEqualityComparer<ResponseProperty>, ResponsePropertyComparer>();
    }
}
