using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XrmSync.Logging;
using XrmSync.Model;
using XrmSync.Options;

namespace XrmSync.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXrmSyncConfiguration(this IServiceCollection services, string? configName)
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

        return services;
    }

    public static IServiceCollection AddPluginSyncOptions(
        this IServiceCollection services,
        Func<PluginSyncOptions, PluginSyncOptions> optionsFactory)
    {
        // Build full configuration for validation and saving
        services.AddSingleton(sp =>
        {
            var builder = sp.GetRequiredService<Options.IConfigurationBuilder>();
            var baseConfig = builder.Build();
            var pluginSyncOptions = optionsFactory(baseConfig.Plugin.Sync);

            // Build complete configuration with new sync options
            return baseConfig with
            {
                Plugin = baseConfig.Plugin with
                {
                    Sync = pluginSyncOptions
                }
            };
        });

        // Register IOptions<XrmSyncConfiguration> for validation
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<XrmSyncConfiguration>();
            return Microsoft.Extensions.Options.Options.Create(config);
        });

        // Register specific IOptions<PluginSyncOptions> for services
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<XrmSyncConfiguration>();
            var options = config.Plugin?.Sync ?? throw new Model.Exceptions.XrmSyncException("Plugin sync options are not configured");
            return Microsoft.Extensions.Options.Options.Create(options);
        });

        return services;
    }

    public static IServiceCollection AddPluginAnalysisOptions(
        this IServiceCollection services,
        Func<PluginAnalysisOptions, PluginAnalysisOptions> optionsFactory)
    {
        // Build full configuration for validation and saving
        services.AddSingleton(sp =>
        {
            var builder = sp.GetRequiredService<Options.IConfigurationBuilder>();
            var baseConfig = builder.Build();
            var pluginAnalysisOptions = optionsFactory(baseConfig.Plugin.Analysis);

            // Build complete configuration with new analysis options
            return baseConfig with
            {
                Plugin = baseConfig.Plugin with
                {
                    Analysis = pluginAnalysisOptions
                }
            };
        });

        // Register IOptions<XrmSyncConfiguration> for validation
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<XrmSyncConfiguration>();
            return Microsoft.Extensions.Options.Options.Create(config);
        });

        // Register specific IOptions<PluginAnalysisOptions> for services
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<XrmSyncConfiguration>();
            var options = config.Plugin?.Analysis ?? throw new Model.Exceptions.XrmSyncException("Plugin analysis options are not configured");
            return Microsoft.Extensions.Options.Options.Create(options);
        });

        return services;
    }

    public static IServiceCollection AddWebresourceSyncOptions(
        this IServiceCollection services,
        Func<WebresourceSyncOptions, WebresourceSyncOptions> optionsFactory)
    {
        // Build full configuration for validation and saving
        services.AddSingleton(sp =>
        {
            var builder = sp.GetRequiredService<Options.IConfigurationBuilder>();
            var baseConfig = builder.Build();
            var webresourceSyncOptions = optionsFactory(baseConfig.Webresource.Sync);
            // Build complete configuration with new sync options
            return baseConfig with
            {
                Webresource = baseConfig.Webresource with
                {
                    Sync = webresourceSyncOptions
                }
            };
        });

        // Register IOptions<XrmSyncConfiguration> for validation
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<XrmSyncConfiguration>();
            return Microsoft.Extensions.Options.Options.Create(config);
        });

        // Register specific IOptions<WebresourceSyncOptions> for services
        services.AddSingleton(sp =>
        {
            var config = sp.GetRequiredService<XrmSyncConfiguration>();
            var options = config.Webresource?.Sync ?? throw new Model.Exceptions.XrmSyncException("Webresource sync options are not configured");
            return Microsoft.Extensions.Options.Options.Create(options);
        });

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
