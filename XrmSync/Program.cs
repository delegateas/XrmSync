using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using XrmSync;
using XrmSync.AssemblyAnalyzer;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Options;
using XrmSync.SyncService;

[assembly: InternalsVisibleTo("Tests")]

var serviceCollection = new ServiceCollection();

var command = new CommandLineBuilder()
    .SetSyncAction(async (cliOptions, cancellationToken) =>
    {
        var (assemblyPath, solutionName, dryRun, logLevel, saveConfig) = cliOptions;

        var serviceProvider = serviceCollection
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

        try
        {
            var options = serviceProvider.GetRequiredService<XrmSyncConfiguration>();

            // Validate options before taking further action
            var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();
            validator.Validate(options, ConfigurationScope.PluginSync);

            // Handle save-config functionality
            if (saveConfig is not null)
            {
                var syncOptions = options.Plugin?.Sync ?? throw new XrmSyncException("No sync configuration loaded - cannot save");
                var configWriter = serviceProvider.GetRequiredService<IConfigWriter>();
                var configPath = string.IsNullOrWhiteSpace(saveConfig) ? null : saveConfig;
                await configWriter.SavePluginSyncConfigAsync(syncOptions, configPath, cancellationToken);
                
                Console.WriteLine($"Configuration saved to {configPath ?? $"{ConfigReader.CONFIG_FILE_BASE}.json"}");
                return true;
            }

            var pluginSync = serviceProvider.GetRequiredService<PluginSyncService>();
            await pluginSync.Sync(cancellationToken);
            return true;
        }
        catch (OptionsValidationException ex)
        {
            Console.Error.WriteLine($"Configuration validation failed:{Environment.NewLine}{ex.Message}");
            return false;
        }
        catch (XrmSyncException ex)
        {
            var log = serviceProvider.GetRequiredService<ILogger>();
            log.LogError("Error during synchronization: {message}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            var log = serviceProvider.GetRequiredService<ILogger>();
            log.LogCritical(ex, "An unexpected error occurred during synchronization");
            return false;
        }
    })
    .SetAnalyzeAction(async (analyzeOptions, cancellationToken) =>
    {
        var (assemblyPath, publisherPrefix, prettyPrint, saveConfig) = analyzeOptions;

        var serviceProvider = serviceCollection
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

        try
        {
            var options = serviceProvider.GetRequiredService<XrmSyncConfiguration>();

            // Validate options before taking further action
            var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();
            validator.Validate(options, ConfigurationScope.PluginAnalysis);

            var analyzisOptions = options.Plugin?.Analysis ?? throw new XrmSyncException("No analysis configuration loaded - cannot proceed");

            // Handle save-config functionality for analyze command
            if (saveConfig is not null)
            {
                var configWriter = serviceProvider.GetRequiredService<IConfigWriter>();

                var configPath = string.IsNullOrWhiteSpace(saveConfig) ? null : saveConfig;
                await configWriter.SaveAnalysisConfigAsync(analyzisOptions, configPath, cancellationToken);
                    
                Console.WriteLine($"Analysis configuration saved to {configPath ?? $"{ConfigReader.CONFIG_FILE_BASE}.json"}");
                return true;
            }

            var analyzer = serviceProvider.GetRequiredService<IAssemblyAnalyzer>();
            var pluginDto = analyzer.AnalyzeAssembly(analyzisOptions.AssemblyPath, analyzisOptions.PublisherPrefix);
            var jsonOptions = new JsonSerializerOptions(JsonSerializerOptions.Default)
            {
                WriteIndented = analyzisOptions.PrettyPrint
            };

            var jsonOutput = JsonSerializer.Serialize(pluginDto, jsonOptions);
            Console.WriteLine(jsonOutput);
            return true;
        }
        catch (OptionsValidationException ex)
        {
            Console.Error.WriteLine($"Configuration validation failed:{Environment.NewLine}{ex.Message}");
            return false;
        }
        catch (AnalysisException ex)
        {
            Console.Error.WriteLine($"Error analyzing assembly: {ex.Message}");
            return false;
        }
    })
    .Build();

var parseResult = command.Parse(args);
return await parseResult.InvokeAsync();
