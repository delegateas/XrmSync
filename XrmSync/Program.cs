using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XrmSync;
using XrmSync.Extensions;
using XrmSync.Model;

[assembly: InternalsVisibleTo("Tests")]

var serviceCollection = new ServiceCollection();

var command = new CommandLineBuilder()
    .SetPluginSyncServiceProviderFactory(opts =>
    {
        var (assemblyPath, solutionName, dryRun, logLevel, ciMode, configName) = opts;

        // Resolve the actual config name to use
        var configReader = new XrmSync.Options.ConfigReader();
        var resolvedConfigName = configReader.ResolveConfigurationName(configName);

        return serviceCollection
            .AddPluginSyncServices()
            .AddXrmSyncConfiguration(resolvedConfigName, builder =>
            {
                var baseOptions = builder.Build();
                var basePluginSyncOptions = baseOptions.Plugin?.Sync;

                var pluginSyncOptions = new PluginSyncOptions(
                    string.IsNullOrWhiteSpace(assemblyPath) ? basePluginSyncOptions?.AssemblyPath ?? string.Empty : assemblyPath,
                    string.IsNullOrWhiteSpace(solutionName) ? basePluginSyncOptions?.SolutionName ?? string.Empty : solutionName,
                    logLevel ?? basePluginSyncOptions?.LogLevel ?? LogLevel.Information,
                    dryRun.GetValueOrDefault() || (basePluginSyncOptions?.DryRun ?? false)
                );

                return new (new (pluginSyncOptions, baseOptions.Plugin?.Analysis));
            })
            .AddLogger(
                sp => sp.GetRequiredService<XrmSyncConfiguration>().Plugin?.Sync?.LogLevel,
                ciMode
            )
            .BuildServiceProvider();
    })
    .SetPluginAnalyzisServiceProviderFactory(opts =>
    {
        var (assemblyPath, publisherPrefix, prettyPrint, configName) = opts;

        // Resolve the actual config name to use
        var configReader = new XrmSync.Options.ConfigReader();
        var resolvedConfigName = configReader.ResolveConfigurationName(configName);

        return serviceCollection
            .AddAnalyzerServices()
            .AddXrmSyncConfiguration(resolvedConfigName, builder =>
            {
                var baseOptions = builder.Build();
                var baseAnalyzerOptions = baseOptions.Plugin?.Analysis;

                var pluginAnalyzisOptions = new PluginAnalysisOptions(
                    string.IsNullOrWhiteSpace(assemblyPath) ? baseAnalyzerOptions?.AssemblyPath ?? string.Empty : assemblyPath,
                    string.IsNullOrWhiteSpace(publisherPrefix) ? baseAnalyzerOptions?.PublisherPrefix ?? string.Empty : publisherPrefix,
                    prettyPrint || (baseAnalyzerOptions?.PrettyPrint ?? false)
                );

                return new (new (baseOptions.Plugin?.Sync, pluginAnalyzisOptions));
            })
            .AddLogger(_ => LogLevel.Information, false)
            .BuildServiceProvider();
    })
    .Build();

var parseResult = command.Parse(args);
return await parseResult.InvokeAsync();
