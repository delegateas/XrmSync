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
/// Root command handler that executes all sync items in a profile
/// </summary>
internal class XrmSyncRootCommand : XrmSyncCommandBase
{
    private readonly List<IXrmSyncCommand> subCommands;
    private readonly Option<bool> dryRun;
    private readonly Option<bool> ciMode;
    private readonly Option<LogLevel?> logLevel;

    public XrmSyncRootCommand(List<IXrmSyncCommand> subCommands)
        : base("xrmsync", "XrmSync - Synchronize your Dataverse plugins and webresources")
    {
        this.subCommands = subCommands;

        // Add override options
        dryRun = new(CliOptions.Execution.DryRun.Primary, CliOptions.Execution.DryRun.Aliases)
        {
            Description = CliOptions.Execution.DryRun.Description,
            Required = false
        };

        ciMode = new(CliOptions.Logging.CiMode.Primary, CliOptions.Logging.CiMode.Aliases)
        {
            Description = CliOptions.Logging.CiMode.Description,
            Required = false
        };

        logLevel = new(CliOptions.Logging.LogLevel.Primary, CliOptions.Logging.LogLevel.Aliases)
        {
            Description = CliOptions.Logging.LogLevel.Description,
            Required = false
        };

        Add(dryRun);
        Add(ciMode);
        Add(logLevel);

        AddSharedOptions();
        SetAction(ExecuteAsync);
    }

    private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var sharedOptions = GetSharedOptionValues(parseResult);

        // Parse override values from CLI
        var dryRunOverride = parseResult.GetValue(dryRun);
        var ciModeOverride = parseResult.GetValue(ciMode);
        var logLevelOverride = parseResult.GetValue(logLevel);

        // Build service provider
        var serviceProvider = new ServiceCollection()
            .AddXrmSyncConfiguration(sharedOptions)
            .AddOptions(baseOptions => baseOptions with
            {
                LogLevel = logLevelOverride ?? baseOptions.LogLevel,
                CiMode = ciModeOverride || baseOptions.CiMode,
                DryRun = dryRunOverride || baseOptions.DryRun
            })
            .AddLogger()
            .BuildServiceProvider();

        var xrmSyncConfig = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<XrmSyncConfiguration>>().Value;
        var logger = serviceProvider.GetRequiredService<ILogger<XrmSyncRootCommand>>();

        // Find the profile
        var profile = xrmSyncConfig.Profiles.FirstOrDefault(p =>
            p.Name.Equals(sharedOptions.ProfileName, StringComparison.OrdinalIgnoreCase));

        if (profile == null)
        {
            logger.LogError("Profile '{profileName}' not found. Use 'xrmsync config list' to see available profiles.", sharedOptions.ProfileName);
            return E_ERROR;
        }

        logger.LogInformation("Running XrmSync with profile: {profileName} (DryRun: {dryRun})",
            profile.Name, xrmSyncConfig.DryRun);

        if (profile.Sync.Count == 0)
        {
            logger.LogWarning("Profile '{profileName}' has no sync items configured. Nothing to execute.", profile.Name);
            return E_ERROR;
        }

        var success = true;

        // Create argument overrides DTO
        var overrides = new ArgumentOverrides(dryRunOverride, ciModeOverride, logLevelOverride);

        // Execute each sync item in the profile
        foreach (var syncItem in profile.Sync)
        {
            logger.LogInformation("Executing {syncType} sync item...", syncItem.SyncType);

            int result = syncItem switch
            {
                PluginSyncItem plugin => await ExecutePluginSync(plugin, profile, sharedOptions, overrides, xrmSyncConfig),
                PluginAnalysisSyncItem analysis => await ExecutePluginAnalysis(analysis, sharedOptions, overrides, xrmSyncConfig),
                WebresourceSyncItem webresource => await ExecuteWebresourceSync(webresource, profile, sharedOptions, overrides, xrmSyncConfig),
                _ => LogUnknownSyncItemType(logger, syncItem.SyncType)
            };

            success = success && result == E_OK;
        }

        return success ? E_OK : E_ERROR;
    }

    private async Task<int> ExecutePluginSync(
        PluginSyncItem syncItem,
        ProfileConfiguration profile,
        SharedOptions sharedOptions,
        ArgumentOverrides overrides,
        XrmSyncConfiguration config)
    {
        var args = new List<string>
        {
            CliOptions.Assembly.Primary, syncItem.AssemblyPath,
            CliOptions.Solution.Primary, profile.SolutionName
        };

        if (!string.IsNullOrWhiteSpace(sharedOptions.ProfileName))
        {
            args.Add(CliOptions.Config.Profile.Primary);
            args.Add(sharedOptions.ProfileName);
        }

        AddCommonArgs(args, overrides, config);
        return await ExecuteSubCommand("plugins", [.. args]);
    }

    private async Task<int> ExecutePluginAnalysis(
        PluginAnalysisSyncItem syncItem,
        SharedOptions sharedOptions,
        ArgumentOverrides overrides,
        XrmSyncConfiguration config)
    {
        var args = new List<string>
        {
            CliOptions.Assembly.Primary, syncItem.AssemblyPath,
            CliOptions.Analysis.Prefix.Primary, syncItem.PublisherPrefix
        };

        if (!string.IsNullOrWhiteSpace(sharedOptions.ProfileName))
        {
            args.Add(CliOptions.Config.Profile.Primary);
            args.Add(sharedOptions.ProfileName);
        }

        if (syncItem.PrettyPrint)
            args.Add(CliOptions.Analysis.PrettyPrint.Primary);

        AddCommonArgs(args, overrides, config);
        return await ExecuteSubCommand("analyze", [.. args]);
    }

    private async Task<int> ExecuteWebresourceSync(
        WebresourceSyncItem syncItem,
        ProfileConfiguration profile,
        SharedOptions sharedOptions,
        ArgumentOverrides overrides,
        XrmSyncConfiguration config)
    {
        var args = new List<string>
        {
            CliOptions.Webresource.Primary, syncItem.FolderPath,
            CliOptions.Solution.Primary, profile.SolutionName
        };

        if (!string.IsNullOrWhiteSpace(sharedOptions.ProfileName))
        {
            args.Add(CliOptions.Config.Profile.Primary);
            args.Add(sharedOptions.ProfileName);
        }

        AddCommonArgs(args, overrides, config);
        return await ExecuteSubCommand("webresources", [.. args]);
    }

    private static void AddCommonArgs(List<string> args, ArgumentOverrides overrides, XrmSyncConfiguration config)
    {
        // Use override if provided, otherwise use config value
        if (overrides.DryRun || config.DryRun)
            args.Add(CliOptions.Execution.DryRun.Primary);

        if (overrides.CiMode || config.CiMode)
            args.Add(CliOptions.Logging.CiMode.Aliases[0]);

        var logLevel = overrides.LogLevel ?? config.LogLevel;
        args.AddRange([CliOptions.Logging.LogLevel.Primary, logLevel.ToString()]);
    }

    private static int LogUnknownSyncItemType(ILogger logger, string syncType)
    {
        logger.LogError("Unknown sync item type: {syncType}", syncType);
        return E_ERROR;
    }

    private async Task<int> ExecuteSubCommand(string commandName, string[] args)
    {
        var command = subCommands.FirstOrDefault(c => c.GetCommand().Name == commandName);
        if (command == null)
        {
            Console.Error.WriteLine($"Sub-command '{commandName}' not found.");
            return E_ERROR;
        }

        var parseResult = command.GetCommand().Parse(args);
        return await parseResult.InvokeAsync();
    }
}
