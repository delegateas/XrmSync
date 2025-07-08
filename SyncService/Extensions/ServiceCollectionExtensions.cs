using Microsoft.Extensions.DependencyInjection;
using XrmSync.SyncService.Comparers;
using XrmSync.SyncService.Differences;
using XrmSync.SyncService.PluginValidator;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;

namespace XrmSync.SyncService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSyncService(this IServiceCollection services)
    {
        return services
            .AddSingleton<Description>()
            .AddSingleton<PluginSyncService>()
            .AddSingleton<IDifferenceUtility, DifferenceUtility>()
            .AddSingleton<IPluginValidator, PluginValidator.PluginValidator>()
            .AddSingleton<IEntityComparer<PluginType>, PluginTypeComparer>()
            .AddSingleton<IEntityComparer<Step>, PluginStepComparer>()
            .AddSingleton<IEntityComparer<Image>, PluginImageComparer>()
            .AddSingleton<IEntityComparer<ApiDefinition>, CustomApiComparer>()
            .AddSingleton<IEntityComparer<RequestParameter>, RequestParameterComparer>()
            .AddSingleton<IEntityComparer<ResponseProperty>, ResponsePropertyComparer>();
    }
}
