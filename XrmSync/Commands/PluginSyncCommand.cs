using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Options;

namespace XrmSync.Commands;

internal record SyncPluginCLIOptions(string? AssemblyPath, string? SolutionName, bool? DryRun, LogLevel? LogLevel, bool CIMode, string? ConfigName);

internal class PluginSyncCommand : XrmSyncCommandBase
{
    private readonly Option<string> _assemblyFile;
    private readonly Option<string> _solutionName;
    private readonly Option<bool> _dryRun;
    private readonly Option<LogLevel?> _logLevel;
    private readonly Option<bool> _saveConfig;
    private readonly Option<string?> _saveConfigTo;
    private readonly Option<bool> _ciMode;
    private readonly Option<string?> _configName;

    public PluginSyncCommand() : base("plugins", "Synchronize plugins in a plugin assembly with Dataverse")
    {
        _assemblyFile = new("--assembly", "--assembly-file", "-a", "--af")
        {
            Description = "Path to the plugin assembly (*.dll)",
            Arity = ArgumentArity.ExactlyOne
        };

        _solutionName = new("--solution", "--solution-name", "--sn", "-n")
        {
            Description = "Name of the solution",
            Arity = ArgumentArity.ExactlyOne
        };

        _dryRun = new("--dry-run", "--dryrun", "--dr")
        {
            Description = "Perform a dry run without making changes",
            Required = false
        };

        _logLevel = new("--log-level", "-l")
        {
            Description = "Set the minimum log level (Trace, Debug, Information, Warning, Error, Critical) (Default: Information)"
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

        _ciMode = new("--ci", "--ci-mode")
        {
            Description = "Enable CI mode which prefixes all warnings and errors for easier parsing in CI systems",
            Required = false
        };

        _configName = new("--config", "--config-name", "-c")
        {
            Description = "Name of the configuration to load from appsettings.json (Default: 'default' or single config if only one exists)",
            Required = false
        };

        Add(_assemblyFile);
        Add(_solutionName);
        Add(_dryRun);
        Add(_logLevel);
        Add(_saveConfig);
        Add(_saveConfigTo);
        Add(_ciMode);
        Add(_configName);

        SetAction(ExecuteAsync);
    }

    private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var assemblyPath = parseResult.GetValue(_assemblyFile);
        var solutionName = parseResult.GetValue(_solutionName);
        var dryRun = parseResult.GetValue(_dryRun);
        var logLevel = parseResult.GetValue(_logLevel);
        var saveConfig = parseResult.GetValue(_saveConfig);
        var saveConfigTo = saveConfig ? parseResult.GetValue(_saveConfigTo) ?? ConfigReader.CONFIG_FILE_BASE + ".json" : null;
        var ciMode = parseResult.GetValue(_ciMode);
        var configName = parseResult.GetValue(_configName);

        var cliOptions = new SyncPluginCLIOptions(assemblyPath, solutionName, dryRun, logLevel, ciMode, configName);
        
        // Build service provider
        var configReader = new ConfigReader();
        var resolvedConfigName = configReader.ResolveConfigurationName(configName);

        var serviceProvider = new ServiceCollection()
            .AddPluginSyncServices()
            .AddXrmSyncConfiguration(resolvedConfigName, builder =>
            {
                var baseOptions = builder.Build();
                var basePluginSyncOptions = baseOptions.Plugin?.Sync;

                var pluginSyncOptions = new PluginSyncOptions(
                    string.IsNullOrWhiteSpace(assemblyPath) ? basePluginSyncOptions?.AssemblyPath ?? string.Empty : assemblyPath,
                    string.IsNullOrWhiteSpace(solutionName) ? basePluginSyncOptions?.SolutionName ?? string.Empty : solutionName,
                    logLevel ?? basePluginSyncOptions?.LogLevel ?? LogLevel.Information,
                    dryRun || (basePluginSyncOptions?.DryRun ?? false)
                );

                return new XrmSyncConfiguration(new PluginOptions(pluginSyncOptions, baseOptions.Plugin?.Analysis));
            })
            .AddLogger(
                sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<XrmSyncConfiguration>>().Value.Plugin?.Sync?.LogLevel,
                ciMode
            )
            .BuildServiceProvider();

        return await RunAction(serviceProvider, saveConfigTo, ConfigurationScope.PluginSync, cancellationToken)
            ? E_OK
            : E_ERROR;
    }
}
