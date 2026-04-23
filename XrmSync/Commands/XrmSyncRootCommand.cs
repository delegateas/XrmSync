using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using XrmSync.Constants;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Options;
using MSOptions = Microsoft.Extensions.Options.Options;

namespace XrmSync.Commands;

/// <summary>
/// Root command handler that executes all sync items in a profile
/// </summary>
internal class XrmSyncRootCommand : XrmSyncCommandBase
{
	private readonly List<IXrmSyncCommand> subCommands;
	private readonly Option<bool?> dryRun;
	private readonly Option<bool?> ciMode;
	private readonly Option<LogLevel?> logLevel;
	private readonly Option<string?> assembly;
	private readonly Option<string?> solution;
	private readonly Option<string?> folder;
	private readonly Option<string[]?> fileExtensions;
	private readonly Option<string?> prefix;
	private readonly Option<IdentityOperation?> operation;
	private readonly Option<string?> clientId;
	private readonly Option<string?> tenantId;

	public XrmSyncRootCommand(List<IXrmSyncCommand> subCommands)
		: base("xrmsync", "XrmSync - Synchronize your Dataverse plugins and webresources")
	{
		this.subCommands = subCommands;

		dryRun = CliOptions.Execution.DryRun.CreateOption<bool?>();
		ciMode = CliOptions.Logging.CiMode.CreateOption<bool?>();
		logLevel = CliOptions.Logging.LogLevel.CreateOption<LogLevel?>();
		assembly = CliOptions.Assembly.CreateOption<string?>();
		solution = CliOptions.Solution.CreateOption<string?>();
		folder = CliOptions.Webresource.CreateOption<string?>();
		fileExtensions = CliOptions.FileExtensions.CreateOption<string[]?>();
		prefix = CliOptions.Analysis.Prefix.CreateOption<string?>();
		operation = CliOptions.ManagedIdentity.Operation.CreateOption<IdentityOperation?>();
		clientId = CliOptions.ManagedIdentity.ClientId.CreateOption<string?>();
		tenantId = CliOptions.ManagedIdentity.TenantId.CreateOption<string?>();

		Add(dryRun);
		Add(ciMode);
		Add(logLevel);
		Add(assembly);
		Add(solution);
		Add(folder);
		Add(fileExtensions);
		Add(prefix);
		Add(operation);
		Add(clientId);
		Add(tenantId);

		AddSharedOptions();
		SetAction(ExecuteAsync);
	}

	private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
	{
		var sharedOptions = GetSharedOptionValues(parseResult);

		var dryRunOverride = parseResult.GetValue(dryRun);
		var ciModeOverride = parseResult.GetValue(ciMode);
		var logLevelOverride = parseResult.GetValue(logLevel);
		var assemblyOverride = parseResult.GetValue(assembly);
		var solutionOverride = parseResult.GetValue(solution);
		var folderOverride = parseResult.GetValue(folder);
		var fileExtensionsOverride = parseResult.GetValue(fileExtensions);
		var prefixOverride = parseResult.GetValue(prefix);
		var operationOverride = parseResult.GetValue(operation);
		var clientIdOverride = parseResult.GetValue(clientId);
		var tenantIdOverride = parseResult.GetValue(tenantId);

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
			Console.Error.WriteLine("No profiles configured. Add a profile to appsettings.json or run 'xrmsync config list'.");
			return E_ERROR;
		}

		// Merge CLI overrides into each sync item
		var mergedSync = rawProfile.Sync.ConvertAll(item => item switch
		{
			PluginSyncItem plugin => plugin with
			{
				AssemblyPath = assemblyOverride.GetValueOrDefault(plugin.AssemblyPath)
			},
			PluginAnalysisSyncItem analysis => analysis with
			{
				AssemblyPath = assemblyOverride.GetValueOrDefault(analysis.AssemblyPath),
				PublisherPrefix = prefixOverride.GetValueOrDefault(analysis.PublisherPrefix)
			},
			WebresourceSyncItem webresource => webresource with
			{
				FolderPath = folderOverride.GetValueOrDefault(webresource.FolderPath),
				FileExtensions = fileExtensionsOverride is { Length: > 0 } ? fileExtensionsOverride.ToList() : webresource.FileExtensions
			},
			IdentitySyncItem identity => identity with
			{
				Operation = operationOverride ?? identity.Operation,
				AssemblyPath = assemblyOverride.GetValueOrDefault(identity.AssemblyPath),
				ClientId = clientIdOverride.GetValueOrDefault(identity.ClientId),
				TenantId = tenantIdOverride.GetValueOrDefault(identity.TenantId)
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
			.AddSingleton(MSOptions.Create(sharedOptions))
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

		var ctx = new ProfileExecutionContext(
			SolutionName: mergedProfile.SolutionName,
			DryRun: mergedConfig.DryRun,
			CiMode: mergedConfig.CiMode,
			LogLevel: mergedConfig.LogLevel,
			ProfileName: sharedOptions.ProfileName);

		var success = true;

		foreach (var syncItem in mergedProfile.Sync)
		{
			logger.LogInformation("Executing {syncType} sync item...", syncItem.SyncType);

			int? result = null;
			foreach (var cmd in subCommands)
			{
				result = await cmd.ExecuteFromProfile(syncItem, ctx, cancellationToken);
				if (result.HasValue) break;
			}

			if (!result.HasValue)
			{
				logger.LogError("Unknown sync item type: {syncType}", syncItem.SyncType);
				success = false;
			}
			else
			{
				success = success && result.Value == E_OK;
			}
		}

		return success ? E_OK : E_ERROR;
	}
}
