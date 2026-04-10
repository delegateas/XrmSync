using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using XrmSync.Constants;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Options;
using XrmSync.SyncService;
using MSOptions = Microsoft.Extensions.Options.Options;

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
	private readonly Option<string?> assembly;
	private readonly Option<string?> solution;
	private readonly IReadOnlyList<ProfileOverrideProvider> profileOverrideProviders;

	public XrmSyncRootCommand(List<IXrmSyncCommand> subCommands)
		: base("xrmsync", "XrmSync - Synchronize your Dataverse plugins and webresources")
	{
		this.subCommands = subCommands;

		// Add override options
		dryRun = CliOptions.Execution.DryRun.CreateOption<bool>();
		ciMode = CliOptions.Logging.CiMode.CreateOption<bool>();
		logLevel = CliOptions.Logging.LogLevel.CreateOption<LogLevel?>();

		// Shared options owned by root command, passed into command override providers
		assembly = CliOptions.Assembly.CreateOption<string?>();
		solution = CliOptions.Solution.CreateOption<string?>();

		Add(dryRun);
		Add(ciMode);
		Add(logLevel);
		Add(assembly);
		Add(solution);

		// Discover and register override options from sub-commands
		var providers = new List<ProfileOverrideProvider>();
		foreach (var cmd in subCommands)
		{
			var provider = cmd.GetProfileOverrides(assembly, solution);
			if (provider == null) continue;
			providers.Add(provider);
			foreach (var option in provider.Options)
				Add(option);
		}
		profileOverrideProviders = providers.AsReadOnly();

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

		// Load raw profile and config from appsettings
		ProfileConfiguration? rawProfile;
		XrmSyncConfiguration rawConfig;
		try
		{
			(rawProfile, rawConfig) = LoadProfileAndConfig(sharedOptions.ProfileName);
		}
		catch (Model.Exceptions.XrmSyncException ex)
		{
			Console.Error.WriteLine(ex.Message);
			return E_ERROR;
		}

		if (rawProfile == null)
		{
			Console.Error.WriteLine($"Profile '{sharedOptions.ProfileName}' not found. Use 'xrmsync config list' to see available profiles.");
			return E_ERROR;
		}

		// Merge CLI overrides into each sync item via advertised providers
		var mergedSync = rawProfile.Sync.Select(item =>
		{
			foreach (var provider in profileOverrideProviders)
			{
				var merged = provider.MergeSyncItem(item, parseResult);
				if (merged != null) return merged;
			}
			return item;
		}).ToList();

		// Apply solution override at profile level
		var solutionOverride = parseResult.GetValue(solution);
		var mergedSolutionName = !string.IsNullOrWhiteSpace(solutionOverride) ? solutionOverride : rawProfile.SolutionName;
		var mergedProfile = rawProfile with { SolutionName = mergedSolutionName, Sync = mergedSync };

		// Build merged config: profile with CLI-overridden sync items + execution mode overrides
		var mergedConfig = rawConfig with
		{
			DryRun = dryRunOverride || rawConfig.DryRun,
			CiMode = ciModeOverride || rawConfig.CiMode,
			LogLevel = logLevelOverride ?? rawConfig.LogLevel,
			Profiles = rawConfig.Profiles
				.Select(p => p.Name == mergedProfile.Name ? mergedProfile : p)
				.ToList()
		};

		// Build DI service provider with merged config
		var serviceProvider = new ServiceCollection()
			.AddSingleton(MSOptions.Create(mergedConfig))
			.AddSingleton(MSOptions.Create(sharedOptions))
			.AddSingleton<IConfigurationValidator, XrmSyncConfigurationValidator>()
			.AddSingleton<IDescription, Description>()
			.AddLogger()
			.BuildServiceProvider();

		var logger = serviceProvider.GetRequiredService<ILogger<XrmSyncRootCommand>>();
		var description = serviceProvider.GetRequiredService<IDescription>();

		logger.LogInformation("{header}", description.ToolHeader);
		logger.LogInformation("Running with profile: {profileName}", mergedProfile.Name);

		if (mergedConfig.DryRun)
		{
			logger.LogInformation("***** DRY RUN *****");
			logger.LogInformation("No changes will be made to Dataverse.");
		}

		if (mergedProfile.Sync.Count == 0)
		{
			logger.LogWarning("Profile '{profileName}' has no sync items configured. Nothing to execute.", mergedProfile.Name);
			return E_ERROR;
		}

		// Validate merged profile configuration upfront before executing any sync items.
		// Identity credentials (ClientId/TenantId) are now included because they may be
		// supplied via CLI overrides (--client-id, --tenant-id) and are already merged above.
		try
		{
			var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();
			validator.Validate(ConfigurationScope.All);
		}
		catch (Exception ex)
		{
			logger.LogCritical("Configuration validation failed — aborting:{nl}{message}", Environment.NewLine, ex.Message);
			return E_ERROR;
		}

		var success = true;

		// Create argument overrides DTO from merged config values
		var overrides = new ArgumentOverrides(mergedConfig.DryRun, mergedConfig.CiMode, mergedConfig.LogLevel);

		// Execute each sync item in the merged profile
		foreach (var syncItem in mergedProfile.Sync)
		{
			logger.LogInformation("Executing {syncType} sync item...", syncItem.SyncType);

			int result = syncItem switch
			{
				PluginSyncItem plugin => await ExecutePluginSync(plugin, mergedProfile, sharedOptions, overrides, mergedConfig),
				PluginAnalysisSyncItem analysis => await ExecutePluginAnalysis(analysis, sharedOptions),
				WebresourceSyncItem webresource => await ExecuteWebresourceSync(webresource, mergedProfile, sharedOptions, overrides, mergedConfig),
				IdentitySyncItem identity => await ExecuteIdentity(identity, mergedProfile, sharedOptions, overrides, mergedConfig),
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
		SharedOptions sharedOptions)
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

		if (syncItem.FileExtensions is { Count: > 0 })
		{
			args.Add(CliOptions.FileExtensions.Primary);
			args.AddRange(syncItem.FileExtensions);
		}

		if (!string.IsNullOrWhiteSpace(sharedOptions.ProfileName))
		{
			args.Add(CliOptions.Config.Profile.Primary);
			args.Add(sharedOptions.ProfileName);
		}

		AddCommonArgs(args, overrides, config);
		return await ExecuteSubCommand("webresources", [.. args]);
	}

	private async Task<int> ExecuteIdentity(
		IdentitySyncItem syncItem,
		ProfileConfiguration profile,
		SharedOptions sharedOptions,
		ArgumentOverrides overrides,
		XrmSyncConfiguration config)
	{
		var args = new List<string>
		{
			CliOptions.ManagedIdentity.Operation.Primary, syncItem.Operation.ToString(),
			CliOptions.Assembly.Primary, syncItem.AssemblyPath,
			CliOptions.Solution.Primary, profile.SolutionName
		};

		if (syncItem.Operation == IdentityOperation.Ensure)
		{
			// Values are guaranteed non-empty at this point (validation passed)
			if (!string.IsNullOrWhiteSpace(syncItem.ClientId))
				args.AddRange([CliOptions.ManagedIdentity.ClientId.Primary, syncItem.ClientId]);
			if (!string.IsNullOrWhiteSpace(syncItem.TenantId))
				args.AddRange([CliOptions.ManagedIdentity.TenantId.Primary, syncItem.TenantId]);
		}

		if (!string.IsNullOrWhiteSpace(sharedOptions.ProfileName))
		{
			args.Add(CliOptions.Config.Profile.Primary);
			args.Add(sharedOptions.ProfileName);
		}

		AddCommonArgs(args, overrides, config);
		return await ExecuteSubCommand("identity", [.. args]);
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
