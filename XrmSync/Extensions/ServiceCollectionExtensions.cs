using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        services.AddSingleton<IAction, PluginSyncAction>();
        services.AddSingleton<ISaveConfigAction, SavePluginSyncConfigAction>();

        services.AddSyncService();
        services.AddAssemblyReader();
        services.AddDataverseConnection();

        return services;
    }

    public static IServiceCollection AddAnalyzerServices(this IServiceCollection services)
    {
        services.AddSingleton<IAction, PluginAnalyzisAction>();
        services.AddSingleton<ISaveConfigAction, SavePluginAnalyzisConfigAction>();

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
