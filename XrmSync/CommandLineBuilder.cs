using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using XrmSync.Actions;
using XrmSync.Commands;
using XrmSync.Model.Exceptions;
using XrmSync.Options;

namespace XrmSync;

internal record SyncPluginCLIOptions(string? AssemblyPath, string? SolutionName, bool? DryRun, LogLevel? LogLevel, bool CIMode, string? ConfigName);
internal record AnalyzeCLIOptions(string? AssemblyPath, string PublisherPrefix, bool PrettyPrint, string? ConfigName);

internal class CommandLineBuilder
{
    protected RootCommand RootCommand { get; init; }
    protected Command SyncPluginsCommand { get; init; }
    protected Command AnalyzeCommand { get; init; }

    private readonly SyncPluginCommandDefinition _syncPluginOptions;
    private readonly AnalyzeCommandDefinition _analyzeOptions;

    private const int E_OK = 0;
    private const int E_ERROR = 1;

    public CommandLineBuilder()
    {
        _syncPluginOptions = new SyncPluginCommandDefinition();
        _analyzeOptions = new AnalyzeCommandDefinition();

        RootCommand = new("XrmSync - Synchronize your Dataverse plugins and webresources");
        
        SyncPluginsCommand = new ("plugins", "Synchronize plugins in a plugin assembly with Dataverse");
        foreach (var option in _syncPluginOptions.GetOptions())
        {
            SyncPluginsCommand.Add(option);
        }

        AnalyzeCommand = new("analyze", "Analyze a plugin assembly and output info as JSON");
        foreach (var option in _analyzeOptions.GetOptions())
        {
            AnalyzeCommand.Add(option);
        }

        RootCommand.Subcommands.Add(AnalyzeCommand);
        RootCommand.Subcommands.Add(SyncPluginsCommand);
    }

    public CommandLineBuilder SetPluginSyncServiceProviderFactory(Func<SyncPluginCLIOptions, IServiceProvider> factory)
    {
        SyncPluginsCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var assemblyPath = parseResult.GetValue(_syncPluginOptions.AssemblyFile);
            var solutionName = parseResult.GetValue(_syncPluginOptions.SolutionName);
            var dryRun = parseResult.GetValue(_syncPluginOptions.DryRun);
            var logLevel = parseResult.GetValue(_syncPluginOptions.LogLevel);
            var saveConfig = parseResult.GetValue(_syncPluginOptions.SaveConfig);
            var saveConfigTo = saveConfig ? parseResult.GetValue(_syncPluginOptions.SaveConfigTo) ?? ConfigReader.CONFIG_FILE_BASE + ".json" : null;
            var ciMode = parseResult.GetValue(_syncPluginOptions.CIMode);
            var configName = parseResult.GetValue(_syncPluginOptions.ConfigName);

            var syncOptions = new SyncPluginCLIOptions(assemblyPath, solutionName, dryRun, logLevel, ciMode, configName);
            var serviceProvider = factory.Invoke(syncOptions);

            return await RunAction(serviceProvider, saveConfigTo, ConfigurationScope.PluginSync, cancellationToken)
                ? E_OK
                : E_ERROR;
        });

        return this;
    }

    public CommandLineBuilder SetPluginAnalyzisServiceProviderFactory(Func<AnalyzeCLIOptions, IServiceProvider> factory)
    {
        AnalyzeCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var assemblyPath = parseResult.GetValue(_analyzeOptions.AssemblyFile);
            var publisherPrefix = parseResult.GetValue(_analyzeOptions.Prefix);
            var prettyPrint = parseResult.GetValue(_analyzeOptions.PrettyPrint);
            var saveConfig = parseResult.GetValue(_analyzeOptions.SaveConfig);
            var saveConfigTo = saveConfig ? parseResult.GetValue(_analyzeOptions.SaveConfigTo) ?? ConfigReader.CONFIG_FILE_BASE + ".json" : null;
            var configName = parseResult.GetValue(_analyzeOptions.ConfigName);

            var analyzeOptions = new AnalyzeCLIOptions(assemblyPath, publisherPrefix ?? "new", prettyPrint, configName);
            var serviceProvider = factory.Invoke(analyzeOptions);

            return await RunAction(serviceProvider, saveConfigTo, ConfigurationScope.PluginAnalysis, cancellationToken)
                ? E_OK
                : E_ERROR;
        });

        return this;
    }

    private static async Task<bool> RunAction(IServiceProvider serviceProvider, string? saveConfig, ConfigurationScope configurationScope, CancellationToken cancellationToken)
    {
        // Validate options before taking further action
        try
        {
            var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();
            validator.Validate(configurationScope);
        }
        catch (OptionsValidationException ex)
        {
            Console.Error.WriteLine($"Configuration validation failed:{Environment.NewLine}{ex.Message}");
            return false;
        }

        if (saveConfig is not null)
        {
            var action = serviceProvider.GetRequiredService<ISaveConfigAction>();
            return await action.SaveConfigAsync(saveConfig, cancellationToken);
        }
        else
        {
            var action = serviceProvider.GetRequiredService<IAction>();
            return await action.RunAction(cancellationToken);
        }
    }

    public RootCommand Build()
    {
        return RootCommand;
    }
}
