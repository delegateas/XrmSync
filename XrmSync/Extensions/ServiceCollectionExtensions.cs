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
        return services.AddSingleton<IConfiguration>(_ =>
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
        });
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

    public static IServiceCollection AddConfigWriter(this IServiceCollection services)
    {
        return services.AddSingleton<IConfigWriter, ConfigWriter>();
    }

    public static IServiceCollection AddLogger(this IServiceCollection services, Func<IServiceProvider, LogLevel> logLevel)
    {
        return services.AddSingleton(sp => LoggerFactory.CreateLogger<ISyncService>(logLevel(sp)));
    }
}
