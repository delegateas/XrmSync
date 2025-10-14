using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using XrmSync.Actions;
using XrmSync.AssemblyAnalyzer.Extensions;
using XrmSync.Extensions;
using XrmSync.Model;
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
        var (saveConfig, saveConfigTo, configName) = GetSharedOptionValues(parseResult);

        // Build service provider
        var configReader = new ConfigReader();
        var resolvedConfigName = configReader.ResolveConfigurationName(configName);

        var serviceProvider = GetAnalyzerServices()
            .AddXrmSyncConfiguration(resolvedConfigName)
            .AddPluginAnalysisOptions(baseOptions =>
            {
                // Merge CLI arguments with file configuration
                return new PluginAnalysisOptions(
                    string.IsNullOrWhiteSpace(assemblyPath) ? baseOptions.AssemblyPath : assemblyPath,
                    string.IsNullOrWhiteSpace(publisherPrefix) ? baseOptions.PublisherPrefix : publisherPrefix,
                    prettyPrint || baseOptions.PrettyPrint
                );
            })
            .AddLogger(_ => LogLevel.Information, false)
            .BuildServiceProvider();

        return await RunAction(serviceProvider, saveConfigTo, ConfigurationScope.PluginAnalysis, cancellationToken)
            ? E_OK
            : E_ERROR;
    }

    private static IServiceCollection GetAnalyzerServices(IServiceCollection? services = null)
    {
        services ??= new ServiceCollection();
        services.AddSingleton<IAction, PluginAnalysisAction>();
        services.AddSingleton<ISaveConfigAction, SavePluginAnalysisConfigAction>();

        services.AddAssemblyAnalyzer();

        return services;
    }
}
