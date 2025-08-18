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
    .SetSyncAction(async (assemblyPath, solutionName, dryRun, logLevel, saveConfig, cancellationToken) =>
    {
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
    .SetAnalyzeAction(async (assemblyPath, publisherPrefix, prettyPrint, cancellationToken) =>
    {
        var serviceProvider = serviceCollection
            .AddAnalyzerServices()
            .AddLogger(_ => LogLevel.Information)
            .BuildServiceProvider();

        return await Task.Run(() =>
        {
            if (string.IsNullOrWhiteSpace(assemblyPath))
            {
                Console.Error.WriteLine("Assembly path is required for analysis.");
                return false;
            }

            try
            {
                var analyzer = serviceProvider.GetRequiredService<IAssemblyAnalyzer>();
                var pluginDto = analyzer.AnalyzeAssembly(assemblyPath, publisherPrefix ?? "new");
                var options = new JsonSerializerOptions(JsonSerializerOptions.Default)
                {
                    WriteIndented = prettyPrint
                };

                var jsonOutput = JsonSerializer.Serialize(pluginDto, options);
                Console.WriteLine(jsonOutput);
                return true;
            }
            catch (AnalysisException ex)
            {
                Console.Error.WriteLine($"Error analyzing assembly: {ex.Message}");
                return false;
            }
        }, cancellationToken);
    })
    .Build();

var parseResult = command.Parse(args);
return await parseResult.InvokeAsync();
