using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Options;

namespace XrmSync.Commands;

internal record AnalyzeCLIOptions(string? AssemblyPath, string PublisherPrefix, bool PrettyPrint, string? ConfigName);

internal class PluginAnalyzeCommand : XrmSyncCommandBase
{

    private readonly Option<string> _assemblyFile;
    private readonly Option<string> _prefix;
    private readonly Option<bool> _prettyPrint;
    private readonly Option<bool> _saveConfig;
    private readonly Option<string?> _saveConfigTo;
    private readonly Option<string?> _configName;

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

        _saveConfig = new("--save-config", "--sc")
        {
            Description = "Save current CLI options to appsettings.json",
            Required = false
        };

        _saveConfigTo = new("--save-config-to")
        {
            Description = "If --save-config is set, save to this file instead of appsettings.json",
            Required = false
        };

        _configName = new("--config", "--config-name", "-c")
        {
            Description = "Name of the configuration to load from appsettings.json (Default: 'default' or single config if only one exists)",
            Required = false
        };

        Add(_assemblyFile);
        Add(_prefix);
        Add(_prettyPrint);
        Add(_saveConfig);
        Add(_saveConfigTo);
        Add(_configName);

        SetAction(ExecuteAsync);
    }

    private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var assemblyPath = parseResult.GetValue(_assemblyFile);
        var publisherPrefix = parseResult.GetValue(_prefix);
        var prettyPrint = parseResult.GetValue(_prettyPrint);
        var saveConfig = parseResult.GetValue(_saveConfig);
        var saveConfigTo = saveConfig ? parseResult.GetValue(_saveConfigTo) ?? ConfigReader.CONFIG_FILE_BASE + ".json" : null;
        var configName = parseResult.GetValue(_configName);

        var cliOptions = new AnalyzeCLIOptions(assemblyPath, publisherPrefix ?? "new", prettyPrint, configName);

        // Build service provider
        var configReader = new ConfigReader();
        var resolvedConfigName = configReader.ResolveConfigurationName(configName);

        var serviceProvider = new ServiceCollection()
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

                return new XrmSyncConfiguration(new PluginOptions(baseOptions.Plugin?.Sync, pluginAnalyzisOptions));
            })
            .AddLogger(_ => LogLevel.Information, false)
            .BuildServiceProvider();

        return await RunAction(serviceProvider, saveConfigTo, ConfigurationScope.PluginAnalysis, cancellationToken)
            ? E_OK
            : E_ERROR;
    }
}
