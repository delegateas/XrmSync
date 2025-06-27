using Microsoft.Extensions.DependencyInjection;

namespace DG.XrmPluginSync.Dataverse.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataverse(this IServiceCollection services)
    {
        DataverseConnection.ServiceCollectionExtensions.AddDataverse(services);
        services.AddSingleton<CrmDataHelper>();
        services.AddSingleton<Solution>();

        return services;
    }
}
