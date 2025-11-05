using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using XrmSync.Constants;
using XrmSync.Extensions;
using XrmSync.Model;

namespace XrmSync.Commands;

/// <summary>
/// DTO for argument overrides when building sub-command arguments
/// </summary>
/// <param name="DryRun">Override for dry run mode</param>
/// <param name="CiMode">Override for CI mode</param>
/// <param name="LogLevel">Override for log level</param>
internal record ArgumentOverrides(bool DryRun, bool CiMode, LogLevel? LogLevel);

/// <summary>
/// Root command handler that executes all configured sub-commands for a given configuration
/// </summary>
internal class XrmSyncRootCommand : XrmSyncCommandBase
{
    private readonly List<IXrmSyncCommand> _subCommands;
    private readonly Option<bool> _dryRun;
    private readonly Option<bool> _ciMode;
    private readonly Option<LogLevel?> _logLevel;

    public XrmSyncRootCommand(List<IXrmSyncCommand> subCommands)
        : base("xrmsync", "XrmSync - Synchronize your Dataverse plugins and webresources")
    {
        _subCommands = subCommands;

        // Add override options
        _dryRun = new(CliOptions.Execution.DryRun.Primary, CliOptions.Execution.DryRun.Aliases)
        {
            Description = CliOptions.Execution.DryRun.Description,
            Required = false
        };

        _ciMode = new(CliOptions.Logging.CiMode.Primary, CliOptions.Logging.CiMode.Aliases)
        {
            Description = CliOptions.Logging.CiMode.Description,
            Required = false
        };

        _logLevel = new(CliOptions.Logging.LogLevel.Primary, CliOptions.Logging.LogLevel.Aliases)
        {
            Description = CliOptions.Logging.LogLevel.Description,
            Required = false
        };

        Add(_dryRun);
        Add(_ciMode);
        Add(_logLevel);

        AddSharedOptions();
        SetAction(ExecuteAsync);
    }

    private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var sharedOptions = GetSharedOptionValues(parseResult);

        // Parse override values from CLI
        var dryRunOverride = parseResult.GetValue(_dryRun);
        var ciModeOverride = parseResult.GetValue(_ciMode);
        var logLevelOverride = parseResult.GetValue(_logLevel);

        // Build service provider using the same pattern as other commands
        var serviceProvider = new ServiceCollection()
            .AddXrmSyncConfiguration(sharedOptions)
            .AddOptions(baseOptions => baseOptions with
            {
                Logger = baseOptions.Logger with
                {
                    LogLevel = logLevelOverride ?? baseOptions.Logger.LogLevel,
                    CiMode = ciModeOverride || baseOptions.Logger.CiMode
                },
                Execution = baseOptions.Execution with
                {
                    DryRun = dryRunOverride || baseOptions.Execution.DryRun
                }
            })
            .AddLogger()
            .BuildServiceProvider();

        var xrmSyncConfig = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<XrmSyncConfiguration>>().Value;
        var logger = serviceProvider.GetRequiredService<ILogger<XrmSyncRootCommand>>();

        logger.LogInformation("Running XrmSync with configuration: {configName} (DryRun: {dryRun})",
            sharedOptions.ConfigName, xrmSyncConfig.Execution.DryRun);

        var success = true;
        var executedAnyCommand = false;

        // Create argument overrides DTO
        var overrides = new ArgumentOverrides(dryRunOverride, ciModeOverride, logLevelOverride);

        // Execute plugin sync if configured
        if (!string.IsNullOrWhiteSpace(xrmSyncConfig.Plugin.Sync.AssemblyPath) &&
            !string.IsNullOrWhiteSpace(xrmSyncConfig.Plugin.Sync.SolutionName))
        {
            logger.LogInformation("Executing plugin sync...");
            var pluginSyncArgs = BuildPluginSyncArgs(xrmSyncConfig, sharedOptions, overrides);
            var result = await ExecuteSubCommand("plugins", pluginSyncArgs);
            success = success && result == E_OK;
            executedAnyCommand = true;
        }

        // Execute webresource sync if configured
        if (!string.IsNullOrWhiteSpace(xrmSyncConfig.Webresource.Sync.FolderPath) &&
            !string.IsNullOrWhiteSpace(xrmSyncConfig.Webresource.Sync.SolutionName))
        {
            logger.LogInformation("Executing webresource sync...");
            var webresourceSyncArgs = BuildWebresourceSyncArgs(xrmSyncConfig, sharedOptions, overrides);
            var result = await ExecuteSubCommand("webresources", webresourceSyncArgs);
            success = success && result == E_OK;
            executedAnyCommand = true;
        }

        if (!executedAnyCommand)
        {
            logger.LogWarning("No sub-commands configured for configuration '{configName}'. Nothing to execute.", sharedOptions.ConfigName);
            return E_ERROR;
        }

        return success ? E_OK : E_ERROR;
    }

    private async Task<int> ExecuteSubCommand(string commandName, string[] args)
    {
        var command = _subCommands.FirstOrDefault(c => c.GetCommand().Name == commandName);
        if (command == null)
        {
            Console.Error.WriteLine($"Sub-command '{commandName}' not found.");
            return E_ERROR;
        }

        var parseResult = command.GetCommand().Parse(args);
        return await parseResult.InvokeAsync();
    }

    private static string[] BuildPluginSyncArgs(
        XrmSyncConfiguration config,
        SharedOptions sharedOptions,
        ArgumentOverrides overrides)
    {
        var args = new List<string>
        {
            CliOptions.Assembly.Primary, config.Plugin.Sync.AssemblyPath,
            CliOptions.Solution.Primary, config.Plugin.Sync.SolutionName,
            CliOptions.Config.LoadConfig.Primary, sharedOptions.ConfigName
        };

        // Use override if provided, otherwise use config value
        if (overrides.DryRun || config.Execution.DryRun)
            args.Add(CliOptions.Execution.DryRun.Primary);

        if (overrides.CiMode || config.Logger.CiMode)
            args.Add(CliOptions.Logging.CiMode.Aliases[0]);

        var logLevel = overrides.LogLevel ?? config.Logger.LogLevel;
        args.AddRange([CliOptions.Logging.LogLevel.Primary, logLevel.ToString()]);

        return [.. args];
    }

    private static string[] BuildWebresourceSyncArgs(
        XrmSyncConfiguration config,
        SharedOptions sharedOptions,
        ArgumentOverrides overrides)
    {
        var args = new List<string>
        {
            CliOptions.Webresource.Primary, config.Webresource.Sync.FolderPath,
            CliOptions.Solution.Primary, config.Webresource.Sync.SolutionName,
            CliOptions.Config.LoadConfig.Primary, sharedOptions.ConfigName
        };

        // Use override if provided, otherwise use config value
        if (overrides.DryRun || config.Execution.DryRun)
            args.Add(CliOptions.Execution.DryRun.Primary);

        if (overrides.CiMode || config.Logger.CiMode)
            args.Add(CliOptions.Logging.CiMode.Aliases[0]);

        var logLevel = overrides.LogLevel ?? config.Logger.LogLevel;
        args.AddRange([CliOptions.Logging.LogLevel.Primary, logLevel.ToString()]);

        return [.. args];
    }
}
