using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XrmSync.AssemblyAnalyzer.Extensions;
using XrmSync.Dataverse.Extensions;
using XrmSync.Model;
using XrmSync.Options;
using XrmSync.SyncService;
using XrmSync.SyncService.Extensions;

namespace XrmSync.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXrmSyncConfiguration(this IServiceCollection services, Func<IConfigurationBuilder, XrmSyncConfiguration> syncOptionsFactory)
    {
        services
            .AddSingleton<IConfigReader, ConfigReader>()
            .AddSingleton<IConfigWriter, ConfigWriter>()
            .AddSingleton<IConfigurationValidator, XrmSyncConfigurationValidator>()
            .AddSingleton(sp => sp.GetRequiredService<IConfigReader>().GetConfiguration())
            .AddSingleton<IConfigurationBuilder, XrmSyncConfigurationBuilder>()
            .AddSingleton(sp => syncOptionsFactory(sp.GetRequiredService<IConfigurationBuilder>()));

        return services;
    }

    public static IServiceCollection AddPluginSyncServices(this IServiceCollection services)
    {
        services.AddSyncService();
        services.AddAssemblyReader();
        services.AddDataverseConnection();

        return services;
    }

    public static IServiceCollection AddAnalyzerServices(this IServiceCollection services)
    {
        return services.AddAssemblyAnalyzer();
    }

    public static IServiceCollection AddLogger(this IServiceCollection services, Func<IServiceProvider, LogLevel?> logLevel)
    {
        return services.AddSingleton(sp => LoggerFactory.CreateLogger<ISyncService>(logLevel(sp)));
    }
}
