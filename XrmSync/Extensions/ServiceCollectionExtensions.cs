using Microsoft.Extensions.Configuration;
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
    public static IServiceCollection ConfigureXrmSync(this IServiceCollection services)
    {
        return services
            .AddSingleton<IConfigReader, ConfigReader>()
            .AddSingleton<IConfigWriter, ConfigWriter>()
            .AddSingleton(sp => sp.GetRequiredService<IConfigReader>().GetConfiguration());
    }

    public static IServiceCollection AddXrmSyncServices(this IServiceCollection services)
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

    public static IServiceCollection AddXrmSyncOptions(this IServiceCollection services, Func<ISyncOptionsBuilder, XrmSyncOptions> syncOptionsFactory)
    {
        services.AddSingleton<ISyncOptionsBuilder, SimpleSyncOptionsBuilder>();
        services.AddSingleton(sp => syncOptionsFactory(sp.GetRequiredService<ISyncOptionsBuilder>()));

        return services;
    }

    public static IServiceCollection AddAnalysisOptions(this IServiceCollection services, Func<IAnalysisOptionsBuilder, PluginAnalysisOptions> analysisOptionsFactory)
    {
        services.AddSingleton<IAnalysisOptionsBuilder, SimpleAnalysisOptionsBuilder>();
        services.AddSingleton(sp => analysisOptionsFactory(sp.GetRequiredService<IAnalysisOptionsBuilder>()));

        return services;
    }

    public static IServiceCollection AddLogger(this IServiceCollection services, Func<IServiceProvider, LogLevel> logLevel)
    {
        return services.AddSingleton(sp => LoggerFactory.CreateLogger<ISyncService>(logLevel(sp)));
    }
}
