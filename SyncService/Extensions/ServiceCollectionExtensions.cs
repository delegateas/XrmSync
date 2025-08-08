using Microsoft.Extensions.DependencyInjection;
using XrmSync.SyncService.Comparers;
using XrmSync.SyncService.PluginValidator;
using XrmSync.Model.CustomApi;
using XrmSync.Model.Plugin;
using XrmSync.SyncService.Difference;

namespace XrmSync.SyncService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSyncService(this IServiceCollection services)
    {
        return services
            .AddSingleton<PluginSyncService>()
            .AddSingleton<Description>()
            .AddSingleton<IDifferenceUtility, DifferenceUtility>()
            .AddSingleton<IPluginValidator, PluginValidator.PluginValidator>()
            .AddSingleton<IEntityComparer<PluginType>, PluginTypeComparer>()
            .AddSingleton<IEntityComparer<Step>, PluginStepComparer>()
            .AddSingleton<IEntityComparer<Image>, PluginImageComparer>()
            .AddSingleton<IEntityComparer<CustomApiDefinition>, CustomApiComparer>()
            .AddSingleton<IEntityComparer<RequestParameter>, RequestParameterComparer>()
            .AddSingleton<IEntityComparer<ResponseProperty>, ResponsePropertyComparer>();
    }
}
