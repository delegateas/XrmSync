using Microsoft.Extensions.DependencyInjection;

namespace DG.XrmPluginSync.Dataverse.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataverse(this IServiceCollection services)
    {
        DataverseConnection.ServiceCollectionExtensions.AddDataverse(services, opts => opts.DataverseUrl = "https://aarsleff-udv.crm4.dynamics.com");
        services.AddScoped<CrmDataHelper>();
        services.AddScoped<Solution>();

        return services;
    }
}
