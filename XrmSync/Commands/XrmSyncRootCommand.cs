using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Options;
using XrmSync.Watch;
using MSOptions = Microsoft.Extensions.Options.Options;

namespace XrmSync.Commands;

/// <summary>
/// Root command handler that executes all sync items in a profile
/// </summary>
internal class XrmSyncRootCommand : XrmSyncCommandBase
{
	private readonly List<IXrmSyncCommand> subCommands;

	public XrmSyncRootCommand(List<IXrmSyncCommand> subCommands)
		: base("xrmsync", "XrmSync - Synchronize your Dataverse plugins and webresources")
	{
		this.subCommands = subCommands;

		Add(CommandOptions.DryRun);
		Add(CommandOptions.CiMode);
		Add(CommandOptions.LogLevel);
		Add(CommandOptions.Assembly);
		Add(CommandOptions.Solution);
		Add(CommandOptions.Folder);
		Add(CommandOptions.FileExtensions);
		Add(CommandOptions.Prefix);
		Add(CommandOptions.Operation);
		Add(CommandOptions.ClientId);
		Add(CommandOptions.TenantId);
		Add(CommandOptions.AllowEmptyTypes);
		Add(CommandOptions.NoDelete);
		Add(CommandOptions.Watch);

		AddSharedOptions();
		SetAction(ExecuteAsync);
	}

	private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
	{
		var (dryRunOverride, ciModeOverride, logLevelOverride, profileName) = ReadExecutionOverrides(parseResult);
		var assemblyOverride = parseResult.GetValue(CommandOptions.Assembly);
		var solutionOverride = parseResult.GetValue(CommandOptions.Solution);
		var folderOverride = parseResult.GetValue(CommandOptions.Folder);
		var fileExtensionsOverride = parseResult.GetValue(CommandOptions.FileExtensions);
		var prefixOverride = parseResult.GetValue(CommandOptions.Prefix);
		var operationOverride = parseResult.GetValue(CommandOptions.Operation);
		var clientIdOverride = parseResult.GetValue(CommandOptions.ClientId);
		var tenantIdOverride = parseResult.GetValue(CommandOptions.TenantId);
		var allowEmptyTypesOverride = parseResult.GetValue(CommandOptions.AllowEmptyTypes);
		var noDeleteOverride = parseResult.GetValue(CommandOptions.NoDelete);
		var watchOverride = parseResult.GetValue(CommandOptions.Watch);

		ProfileConfiguration? rawProfile;
		XrmSyncConfiguration rawConfig;
		try
		{
			(rawProfile, rawConfig) = LoadProfileAndConfig(profileName);
		}
		catch (Model.Exceptions.XrmSyncException ex)
		{
			Console.Error.WriteLine(ex.Message);
			return E_ERROR;
		}

		if (rawProfile == null)
		{
			Console.Error.WriteLine("No profiles configured. Add a profile to appsettings.json or run 'xrmsync config list'.");
			return E_ERROR;
		}

		// Resolve effective values for each sync item, with precedence:
		//   CLI override → per-item value → profile-level value.
		// The resolved solution name is baked onto each item's SolutionName so it travels with the item.
		string ResolveAssembly(string? itemAssemblyPath) =>
			assemblyOverride.GetValueOrDefault(rawProfile.ResolveAssemblyPath(itemAssemblyPath) ?? string.Empty);
		string ResolveSolution(SyncItem item) =>
			solutionOverride.GetValueOrDefault(rawProfile.ResolveSolutionName(item));

		var mergedSync = rawProfile.Sync.ConvertAll(item => item switch
		{
			PluginSyncItem plugin => plugin with
			{
				AssemblyPath = ResolveAssembly(plugin.AssemblyPath),
				ManagedIdentityClientId = clientIdOverride.GetValueOrDefault(plugin.ManagedIdentityClientId ?? string.Empty),
				ManagedIdentityTenantId = tenantIdOverride.GetValueOrDefault(plugin.ManagedIdentityTenantId ?? string.Empty),
				AllowEmptyTypes = allowEmptyTypesOverride ?? plugin.AllowEmptyTypes,
				NoDelete = noDeleteOverride ?? plugin.NoDelete,
				SolutionName = ResolveSolution(plugin),
				Watch = watchOverride ?? plugin.Watch
			},
			PluginAnalysisSyncItem analysis => analysis with
			{
				AssemblyPath = ResolveAssembly(analysis.AssemblyPath),
				PublisherPrefix = prefixOverride.GetValueOrDefault(analysis.PublisherPrefix)
			},
			WebresourceSyncItem webresource => webresource with
			{
				FolderPath = folderOverride.GetValueOrDefault(webresource.FolderPath),
				FileExtensions = fileExtensionsOverride is { Length: > 0 } ? fileExtensionsOverride.ToList() : webresource.FileExtensions,
				NoDelete = noDeleteOverride ?? webresource.NoDelete,
				SolutionName = ResolveSolution(webresource),
				Watch = watchOverride ?? webresource.Watch
			},
			IdentitySyncItem identity => identity with
			{
				Operation = operationOverride ?? identity.Operation,
				AssemblyPath = ResolveAssembly(identity.AssemblyPath),
				ClientId = clientIdOverride.GetValueOrDefault(identity.ClientId),
				TenantId = tenantIdOverride.GetValueOrDefault(identity.TenantId),
				SolutionName = ResolveSolution(identity)
			},
			_ => item
		});

		var mergedSolutionName = solutionOverride.GetValueOrDefault(rawProfile.SolutionName);
		var mergedProfile = rawProfile with { SolutionName = mergedSolutionName, Sync = mergedSync };

		var mergedConfig = rawConfig with
		{
			DryRun = dryRunOverride ?? rawConfig.DryRun,
			CiMode = ciModeOverride ?? rawConfig.CiMode,
			LogLevel = logLevelOverride ?? rawConfig.LogLevel,
			Profiles = rawConfig.Profiles
				.Select(p => p.Name == mergedProfile.Name ? mergedProfile : p)
				.ToList()
		};

		var serviceProvider = new ServiceCollection()
			.AddSingleton(MSOptions.Create(mergedConfig))
			.AddSingleton(MSOptions.Create(new ExecutionContext(null, mergedConfig.DryRun, mergedConfig.CiMode, mergedConfig.LogLevel, profileName)))
			.AddSingleton<IConfigurationValidator, XrmSyncConfigurationValidator>()
			.AddLogger()
			.BuildServiceProvider();

		var logger = serviceProvider.GetRequiredService<ILogger<XrmSyncRootCommand>>();

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

		try
		{
			var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();
			validator.Validate(ConfigurationScope.All);
		}
		catch (Exception ex)
		{
			logger.LogCritical(ex, "Configuration validation failed — aborting:{nl}{message}", Environment.NewLine, ex.Message);
			return E_ERROR;
		}

		var ctx = new ExecutionContext(
			SolutionName: mergedProfile.SolutionName,
			DryRun: mergedConfig.DryRun,
			CiMode: mergedConfig.CiMode,
			LogLevel: mergedConfig.LogLevel,
			ProfileName: profileName);

		// Resolved before the initial pass so it can tell each item whether it belongs to a watch session
		var watchSettings = WatchSettings.Resolve(watchOverride, mergedProfile.Sync.Any(item => item.Watch), mergedConfig.CiMode, mergedConfig);

		if (watchSettings.Suppressed)
		{
			logger.LogWarning("Watch mode is not supported in CI mode — running once and exiting.");
		}

		var success = true;

		foreach (var syncItem in mergedProfile.Sync)
		{
			// Mirrors how watch targets are selected below, so an item that only runs once is not a watch session
			var itemWatchSession = watchSettings.Enabled && syncItem.Watch;
			var result = await RunSyncItem(syncItem, ctx with { WatchSession = itemWatchSession }, mergedProfile, logger, cancellationToken);
			success = success && result == E_OK;
		}

		if (!watchSettings.Enabled)
		{
			return success ? E_OK : E_ERROR;
		}

		var targets = new List<WatchTarget>();
		foreach (var syncItem in mergedProfile.Sync.Where(item => item.Watch))
		{
			var target = WatchTargetResolver.TryCreate(syncItem, mergedProfile);
			if (target == null)
			{
				logger.LogWarning("{syncType} sync items cannot be watched — it ran once only.", syncItem.SyncType);
			}
			else
			{
				targets.Add(target);
			}
		}

		if (targets.Count == 0)
		{
			logger.LogWarning("No watchable sync items in profile '{profileName}'. Nothing to watch.", mergedProfile.Name);
			return success ? E_OK : E_ERROR;
		}

		var watchLoop = new WatchLoop(new WatchFileSystem(), serviceProvider.GetRequiredService<ILogger<WatchLoop>>(), watchSettings);
		await watchLoop.RunAsync(
			targets,
			(target, ct) => RunSyncItem(target.Item, ctx with { WatchSession = true }, mergedProfile, logger, ct),
			cancellationToken);

		// A watch session's exit code reflects the initial pass — later failures are surfaced in the log
		return success ? E_OK : E_ERROR;
	}

	/// <summary>
	/// Executes a single sync item by handing it to the sub-command that handles its type.
	/// This is the single execution path shared by the initial pass and every watch-triggered re-run,
	/// which is why a re-run can never start a nested watch.
	/// </summary>
	private async Task<int> RunSyncItem(SyncItem syncItem, ExecutionContext ctx, ProfileConfiguration profile, ILogger logger, CancellationToken cancellationToken)
	{
		logger.LogInformation("Executing {syncType} sync item...", syncItem.SyncType);

		// Each item carries its own effective solution name (per-item override or profile-level)
		var itemCtx = ctx with { SolutionName = profile.ResolveSolutionName(syncItem) };

		int? result = null;
		foreach (var cmd in subCommands)
		{
			result = await cmd.ExecuteFromProfile(syncItem, itemCtx, cancellationToken);
			if (result.HasValue) break;
		}

		if (!result.HasValue)
		{
			logger.LogError("Unknown sync item type: {syncType}", syncItem.SyncType);
			return E_ERROR;
		}

		return result.Value;
	}
}
