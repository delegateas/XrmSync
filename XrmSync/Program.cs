using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XrmSync;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Options;
using XrmSync.Actions;

[assembly: InternalsVisibleTo("Tests")]

var serviceCollection = new ServiceCollection();

var command = new CommandLineBuilder()
    .SetPluginSyncServiceProviderFactory(opts =>
    {
        var (assemblyPath, solutionName, dryRun, logLevel) = opts;

        return serviceCollection
            .AddPluginSyncServices()
            .AddXrmSyncConfiguration(builder =>
            {
                var baseOptions = builder.Build();
                var basePluginSyncOptions = baseOptions.Plugin?.Sync;

                var pluginSyncOptions = new PluginSyncOptions(
                    string.IsNullOrWhiteSpace(assemblyPath) ? basePluginSyncOptions?.AssemblyPath ?? string.Empty : assemblyPath,
                    string.IsNullOrWhiteSpace(solutionName) ? basePluginSyncOptions?.SolutionName ?? string.Empty : solutionName,
                    logLevel ?? basePluginSyncOptions?.LogLevel ?? XrmSync.LoggerFactory.DefaultLogLevel,
                    dryRun.GetValueOrDefault() || (baseOptions.Plugin?.Sync?.DryRun ?? false)
                );

                return new (new (pluginSyncOptions, baseOptions.Plugin?.Analysis));
            })
            .AddLogger(sp => sp.GetRequiredService<XrmSyncConfiguration>().Plugin?.Sync?.LogLevel)
            .BuildServiceProvider();
    })
    .SetPluginAnalyzisServiceProviderFactory(opts =>
    {
        var (assemblyPath, publisherPrefix, prettyPrint) = opts;

        return serviceCollection
            .AddAnalyzerServices()
            .AddXrmSyncConfiguration(builder =>
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
            .AddLogger(_ => LogLevel.Information)
            .BuildServiceProvider();
    })
    .Build();

var parseResult = command.Parse(args);
return await parseResult.InvokeAsync();
