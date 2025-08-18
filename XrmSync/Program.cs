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

var serviceCollection = new ServiceCollection()
    .ConfigureXrmSync();

var command = new CommandLineBuilder()
    .SetSyncAction(async (syncOptions, cancellationToken) =>
    {
        var (assemblyPath, solutionName, dryRun, logLevel, saveConfig) = syncOptions;

        var serviceProvider = serviceCollection
            .AddXrmSyncServices()
            .AddXrmSyncOptions(builder =>
            {
                var baseOptions = builder.Build();

                return new XrmSyncOptions(
                    string.IsNullOrWhiteSpace(assemblyPath) ? baseOptions.AssemblyPath : assemblyPath,
                    string.IsNullOrWhiteSpace(solutionName) ? baseOptions.SolutionName : solutionName,
                    logLevel ?? baseOptions.LogLevel,
                    dryRun.GetValueOrDefault() || baseOptions.DryRun
                );
            })
            .AddLogger(sp => sp.GetRequiredService<XrmSyncOptions>().LogLevel)
            .BuildServiceProvider();

        try
        {
            var options = serviceProvider.GetRequiredService<XrmSyncOptions>();
            
            // Handle save-config functionality
            if (saveConfig is not null)
            {
                var configWriter = serviceProvider.GetRequiredService<IConfigWriter>();
                var configPath = string.IsNullOrWhiteSpace(saveConfig) ? null : saveConfig;
                await configWriter.SaveConfigAsync(options, configPath, cancellationToken);
                
                Console.WriteLine($"Configuration saved to {configPath ?? $"{ConfigReader.CONFIG_FILE_BASE}.json"}");
                return true;
            }

            var pluginSync = serviceProvider.GetRequiredService<PluginSyncService>();
            await pluginSync.Sync(cancellationToken);
            return true;
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
            .AddAnalysisOptions(builder =>
            {
                var baseOptions = builder.Build();

                return new PluginAnalysisOptions(
                    string.IsNullOrWhiteSpace(assemblyPath) ? baseOptions.AssemblyPath : assemblyPath,
                    string.IsNullOrWhiteSpace(publisherPrefix) ? baseOptions.PublisherPrefix : publisherPrefix,
                    prettyPrint || baseOptions.PrettyPrint
                );
            })
            .AddLogger(_ => LogLevel.Information)
            .BuildServiceProvider();

        try
        {
            var options = serviceProvider.GetRequiredService<PluginAnalysisOptions>();

            // Handle save-config functionality for analyze command
            if (saveConfig is not null)
            {
                var configWriter = serviceProvider.GetRequiredService<IConfigWriter>();
                var configPath = string.IsNullOrWhiteSpace(saveConfig) ? null : saveConfig;
                await configWriter.SaveAnalysisConfigAsync(options, configPath, cancellationToken);
                    
                Console.WriteLine($"Analysis configuration saved to {configPath ?? $"{ConfigReader.CONFIG_FILE_BASE}.json"}");
                return true;
            }

            if (string.IsNullOrWhiteSpace(options.AssemblyPath))
            {
                Console.Error.WriteLine("Assembly path is required for analysis.");
                return false;
            }

            var analyzer = serviceProvider.GetRequiredService<IAssemblyAnalyzer>();
            var pluginDto = analyzer.AnalyzeAssembly(options.AssemblyPath, options.PublisherPrefix);
            var jsonOptions = new JsonSerializerOptions(JsonSerializerOptions.Default)
            {
                WriteIndented = options.PrettyPrint
            };

            var jsonOutput = JsonSerializer.Serialize(pluginDto, jsonOptions);
            Console.WriteLine(jsonOutput);
            return true;
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
