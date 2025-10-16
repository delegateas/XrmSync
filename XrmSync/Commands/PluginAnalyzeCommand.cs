using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.CommandLine;
using System.Text.Json;
using XrmSync.AssemblyAnalyzer;
using XrmSync.AssemblyAnalyzer.Extensions;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Options;

namespace XrmSync.Commands;

internal class PluginAnalyzeCommand : XrmSyncCommandBase
{

    private readonly Option<string> _assemblyFile;
    private readonly Option<string> _prefix;
    private readonly Option<bool> _prettyPrint;

    public PluginAnalyzeCommand() : base("analyze", "Analyze a plugin assembly and output info as JSON")
    {
        _assemblyFile = new("--assembly", "--assembly-file", "-a", "--af")
        {
            Description = "Path to the plugin assembly (*.dll)",
            Arity = ArgumentArity.ExactlyOne
        };

        _prefix = new("--prefix", "--publisher-prefix", "-p")
        {
            Description = "Publisher prefix for unique names (Default: new)",
            Arity = ArgumentArity.ExactlyOne
        };

        _prettyPrint = new("--pretty-print", "--pp")
        {
            Description = "Pretty print the JSON output",
            Required = false
        };

        Add(_assemblyFile);
        Add(_prefix);
        Add(_prettyPrint);
        AddSharedOptions();

        SetAction(ExecuteAsync);
    }

    private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var assemblyPath = parseResult.GetValue(_assemblyFile);
        var publisherPrefix = parseResult.GetValue(_prefix);
        var prettyPrint = parseResult.GetValue(_prettyPrint);
        var sharedOptions = GetSharedOptionValues(parseResult);

        // Build service provider
        var serviceProvider = GetAnalyzerServices()
            .AddXrmSyncConfiguration(sharedOptions)
            .AddOptions(
                baseOptions => baseOptions with
                {
                    Plugin = baseOptions.Plugin with
                    {
                        Analysis = new(
                            string.IsNullOrWhiteSpace(assemblyPath) ? baseOptions.Plugin.Analysis.AssemblyPath : assemblyPath,
                            string.IsNullOrWhiteSpace(publisherPrefix) ? baseOptions.Plugin.Analysis.PublisherPrefix : publisherPrefix,
                            prettyPrint || baseOptions.Plugin.Analysis.PrettyPrint
                        )
                    }
                }
            )
            .AddCommandOptions(c => c.Plugin.Analysis)
            .AddLogger()
            .BuildServiceProvider();

        return await RunAction(serviceProvider, ConfigurationScope.PluginAnalysis, CommandAction, cancellationToken)
            ? E_OK
            : E_ERROR;
    }

    private static async Task<bool> CommandAction(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            try
            {
                var analyzer = serviceProvider.GetRequiredService<IAssemblyAnalyzer>();
                var configuration = serviceProvider.GetRequiredService<IOptions<PluginAnalysisOptions>>();

                var pluginDto = analyzer.AnalyzeAssembly(configuration.Value.AssemblyPath, configuration.Value.PublisherPrefix);
                var jsonOptions = new JsonSerializerOptions(JsonSerializerOptions.Default)
                {
                    WriteIndented = configuration.Value.PrettyPrint
                };

                var jsonOutput = JsonSerializer.Serialize(pluginDto, jsonOptions);
                Console.WriteLine(jsonOutput);
                return true;
            }
            catch (XrmSyncException ex)
            {
                Console.Error.WriteLine($"Error analyzing assembly: {ex.Message}");
                return false;
            }
        });
    }

    private static IServiceCollection GetAnalyzerServices(IServiceCollection? services = null)
    {
        services ??= new ServiceCollection();

        services.AddAssemblyAnalyzer();

        return services;
    }
}
