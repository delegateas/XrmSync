using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XrmSync.Actions;
using XrmSync.AssemblyAnalyzer.Extensions;
using XrmSync.Dataverse.Extensions;
using XrmSync.Logging;
using XrmSync.Model;
using XrmSync.Options;
using XrmSync.SyncService.Extensions;

namespace XrmSync.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXrmSyncConfiguration(this IServiceCollection services, string? configName, Func<Options.IConfigurationBuilder, XrmSyncConfiguration> syncOptionsFactory)
    {
        services
            .AddSingleton<IConfigReader, ConfigReader>()
            .AddSingleton<IConfigWriter, ConfigWriter>()
            .AddSingleton<IConfigurationValidator, XrmSyncConfigurationValidator>()
            .AddSingleton(sp => sp.GetRequiredService<IConfigReader>().GetConfiguration())
            .AddSingleton<Options.IConfigurationBuilder>(sp => 
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                return new XrmSyncConfigurationBuilder(configuration, configName);
            });

        // Register IOptions<XrmSyncConfiguration> directly from the factory
        services.AddSingleton(sp =>
        {
            var builder = sp.GetRequiredService<Options.IConfigurationBuilder>();
            var config = syncOptionsFactory(builder);
            return Microsoft.Extensions.Options.Options.Create(config);
        });

        return services;
    }

    public static IServiceCollection AddPluginSyncServices(this IServiceCollection services)
    {
        services.AddSingleton<IAction, PluginSyncAction>();
        services.AddSingleton<ISaveConfigAction, SavePluginSyncConfigAction>();

        services.AddSyncService();
        services.AddAssemblyReader();
        services.AddDataverseConnection();

        return services;
    }

    public static IServiceCollection AddAnalyzerServices(this IServiceCollection services)
    {
        services.AddSingleton<IAction, PluginAnalysisAction>();
        services.AddSingleton<ISaveConfigAction, SavePluginAnalysisConfigAction>();

        services.AddAssemblyAnalyzer();

        return services;
    }

    public static IServiceCollection AddLogger(this IServiceCollection services, Func<IServiceProvider, LogLevel?> logLevel, bool ciMode)
    {
        services.AddSingleton(sp =>
            LoggerFactory.Create(builder =>
            {
                builder.AddFilter(nameof(Microsoft), LogLevel.Warning)
                    .AddFilter(nameof(System), LogLevel.Warning)
                    .AddFilter(nameof(XrmSync), logLevel(sp) ?? LogLevel.Information)
                    .AddConsole(options => options.FormatterName = "ci-console")
                    .AddConsoleFormatter<CIConsoleFormatter, CIConsoleFormatterOptions>(options =>
                    {
                        options.IncludeScopes = false;
                        options.SingleLine = true;
                        options.TimestampFormat = "HH:mm:ss ";
                        options.CIMode = ciMode;
                    });
            }));

        services.AddSingleton(typeof(ILogger<>), typeof(SyncLogger<>));

        return services;
    }
}
