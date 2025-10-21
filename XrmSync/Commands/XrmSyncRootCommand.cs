using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using XrmSync.Extensions;
using XrmSync.Model;

namespace XrmSync.Commands;

/// <summary>
/// Root command handler that executes all configured sub-commands for a given configuration
/// </summary>
internal class XrmSyncRootCommand : XrmSyncCommandBase
{
    private readonly List<IXrmSyncCommand> _subCommands;

    public XrmSyncRootCommand(List<IXrmSyncCommand> subCommands)
        : base("xrmsync", "XrmSync - Synchronize your Dataverse plugins and webresources")
    {
        _subCommands = subCommands;
        AddSharedOptions();
        SetAction(ExecuteAsync);
    }

    private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var sharedOptions = GetSharedOptionValues(parseResult);

        // Build service provider using the same pattern as other commands
        var serviceProvider = new ServiceCollection()
            .AddXrmSyncConfiguration(sharedOptions)
            .AddOptions(baseOptions => baseOptions) // No CLI overrides for root command
            .AddLogger()
            .BuildServiceProvider();

        var xrmSyncConfig = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<XrmSyncConfiguration>>().Value;
        var logger = serviceProvider.GetRequiredService<ILogger<XrmSyncRootCommand>>();

        logger.LogInformation("Running XrmSync with configuration: {configName} (DryRun: {dryRun})", 
            sharedOptions.ConfigName, xrmSyncConfig.Execution.DryRun);

        var success = true;
        var executedAnyCommand = false;

        // Execute plugin sync if configured
        if (!string.IsNullOrWhiteSpace(xrmSyncConfig.Plugin.Sync.AssemblyPath) &&
            !string.IsNullOrWhiteSpace(xrmSyncConfig.Plugin.Sync.SolutionName))
        {
            logger.LogInformation("Executing plugin sync...");
            var pluginSyncArgs = BuildPluginSyncArgs(xrmSyncConfig, sharedOptions);
            var result = await ExecuteSubCommand("plugins", pluginSyncArgs);
            success = success && result == E_OK;
            executedAnyCommand = true;
        }

        // Execute webresource sync if configured
        if (!string.IsNullOrWhiteSpace(xrmSyncConfig.Webresource.Sync.FolderPath) &&
            !string.IsNullOrWhiteSpace(xrmSyncConfig.Webresource.Sync.SolutionName))
        {
            logger.LogInformation("Executing webresource sync...");
            var webresourceSyncArgs = BuildWebresourceSyncArgs(xrmSyncConfig, sharedOptions);
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

    private static string[] BuildPluginSyncArgs(XrmSyncConfiguration config, SharedOptions sharedOptions)
    {
        var args = new List<string>
        {
            "--assembly", config.Plugin.Sync.AssemblyPath,
            "--solution", config.Plugin.Sync.SolutionName,
            "--config", sharedOptions.ConfigName
        };

        if (config.Execution.DryRun)
            args.Add("--dry-run");

        if (config.Logger.CiMode)
            args.Add("--ci");

        args.AddRange(new[] { "--log-level", config.Logger.LogLevel.ToString() });

        return args.ToArray();
    }

    private static string[] BuildWebresourceSyncArgs(XrmSyncConfiguration config, SharedOptions sharedOptions)
    {
        var args = new List<string>
        {
            "--folder", config.Webresource.Sync.FolderPath,
            "--solution", config.Webresource.Sync.SolutionName,
            "--config", sharedOptions.ConfigName
        };

        if (config.Execution.DryRun)
            args.Add("--dry-run");

        if (config.Logger.CiMode)
            args.Add("--ci");

        args.AddRange(new[] { "--log-level", config.Logger.LogLevel.ToString() });

        return args.ToArray();
    }
}
