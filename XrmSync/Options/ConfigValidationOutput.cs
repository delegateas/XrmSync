using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using XrmSync.Model;
using XrmSync.Model.Exceptions;

namespace XrmSync.Options;

internal class ConfigValidationOutput(
	IConfiguration configuration,
	IOptions<XrmSyncConfiguration>? configOptions = null,
	IOptions<ExecutionContext>? sharedOptions = null) : IConfigValidationOutput
{
	public Task OutputValidationResult(CancellationToken cancellationToken = default)
	{
		if (configOptions == null || sharedOptions == null)
		{
			throw new InvalidOperationException("ConfigValidationOutput requires XrmSyncConfiguration and ExecutionContext to validate configuration. Use OutputConfigList for listing profiles.");
		}

		var profileName = sharedOptions.Value.ProfileName;
		var configSource = GetConfigurationSource();

		var config = configOptions.Value;

		ProfileConfiguration profile;
		try
		{
			var resolved = config.ResolveProfile(profileName);
			if (resolved == null)
			{
				Console.WriteLine($"No profiles configured in {configSource}");
				return Task.CompletedTask;
			}
			profile = resolved;
		}
		catch (XrmSyncException ex)
		{
			Console.WriteLine($"{ex.Message} Use --all to validate all profiles.");
			return Task.CompletedTask;
		};

		Console.WriteLine($"Profile: '{profile.Name}' (from {configSource})");
		Console.WriteLine();

		var globalValid = OutputGlobalConfiguration(config);

		var allValid = OutputProfileValidation(profile) && globalValid;

		// Final validation status
		if (allValid)
		{
			Console.WriteLine("Validation: PASSED");
		}
		else
		{
			Console.WriteLine("Validation: FAILED - See errors above");
		}

		return Task.CompletedTask;
	}

	public Task OutputAllValidationResults(CancellationToken cancellationToken = default)
	{
		if (configOptions == null)
		{
			throw new InvalidOperationException("ConfigValidationOutput requires XrmSyncConfiguration to validate configuration.");
		}

		var config = configOptions.Value;
		var configSource = GetConfigurationSource();

		if (config.Profiles.Count == 0)
		{
			Console.WriteLine($"No profiles configured in {configSource}");
			return Task.CompletedTask;
		}

		Console.WriteLine($"Validating all profiles (from {configSource})");
		Console.WriteLine();

		// Display global settings once
		var globalValid = OutputGlobalConfiguration(config);

		var profileResults = new List<(string Name, bool Valid)>();

		foreach (var profile in config.Profiles)
		{
			Console.WriteLine(new string('─', 40));
			Console.WriteLine($"Profile: '{profile.Name}'");
			Console.WriteLine();

			var valid = OutputProfileValidation(profile) && globalValid;
			profileResults.Add((profile.Name, valid));
		}

		Console.WriteLine(new string('─', 40));
		Console.WriteLine();

		// Summary
		var passCount = profileResults.Count(r => r.Valid);
		Console.WriteLine($"Summary: {passCount}/{profileResults.Count} profiles passed validation");

		if (passCount < profileResults.Count)
		{
			var failed = profileResults.Where(r => !r.Valid).Select(r => r.Name);
			Console.WriteLine($"Failed: {string.Join(", ", failed)}");
			Console.WriteLine();
			Console.WriteLine("Validation: FAILED");
		}
		else
		{
			Console.WriteLine();
			Console.WriteLine("Validation: PASSED");
		}

		return Task.CompletedTask;
	}

	public Task OutputConfigList(CancellationToken cancellationToken = default)
	{
		var xrmSyncSection = configuration.GetSection(XrmSyncConfigurationBuilder.SectionName.XrmSync);

		if (!xrmSyncSection.Exists())
		{
			Console.WriteLine("No XrmSync configuration found in appsettings.json");
			return Task.CompletedTask;
		}

		var profilesSection = xrmSyncSection.GetSection(XrmSyncConfigurationBuilder.SectionName.Profiles);

		if (!profilesSection.Exists())
		{
			Console.WriteLine("No profiles found in XrmSync configuration");
			return Task.CompletedTask;
		}

		var profiles = profilesSection.GetChildren().ToList();

		if (profiles.Count == 0)
		{
			Console.WriteLine("No profiles found in XrmSync configuration");
			return Task.CompletedTask;
		}

		Console.WriteLine($"Available profiles (from {GetConfigurationSource()}):");
		Console.WriteLine();

		foreach (var profileSection in profiles)
		{
			var name = profileSection.GetValue<string>("Name") ?? string.Empty;
			var solutionName = profileSection.GetValue<string>("SolutionName") ?? string.Empty;
			var syncItems = profileSection.GetSection("Sync").GetChildren().ToList();

			Console.WriteLine($"  - {name}");
			Console.WriteLine($"    Solution: {solutionName}");

			if (syncItems.Count > 0)
			{
				var syncTypes = syncItems
					.Select(s => s.GetValue<string>("Type"))
					.Where(t => !string.IsNullOrEmpty(t))
					.ToList();
				Console.WriteLine($"    Sync Items: {string.Join(", ", syncTypes)} ({syncItems.Count} total)");
			}
			else
			{
				Console.WriteLine($"    Sync Items: None");
			}

			Console.WriteLine();
		}

		return Task.CompletedTask;
	}

	/// <summary>
	/// Prints the global (non-profile) settings block and returns whether they are valid.
	/// </summary>
	private static bool OutputGlobalConfiguration(XrmSyncConfiguration config)
	{
		var errors = XrmSyncConfigurationValidator.ValidateWatchDebounce(config.WatchDebounceMs).ToList();

		Console.WriteLine($"{(errors.Count == 0 ? "✓" : "✗")} Global Configuration");
		Console.WriteLine($"  Dry Run: {config.DryRun}");
		Console.WriteLine($"  Log Level: {config.LogLevel}");
		Console.WriteLine($"  CI Mode: {config.CiMode}");
		Console.WriteLine($"  Watch Debounce: {config.WatchDebounceMs} ms");
		foreach (var error in errors)
		{
			Console.WriteLine($"  Error: {error}");
		}
		Console.WriteLine();

		return errors.Count == 0;
	}

	private bool OutputProfileValidation(ProfileConfiguration profile)
	{
		// Display profile settings
		Console.WriteLine($"✓ Profile '{profile.Name}'");
		Console.WriteLine($"  Solution Name: {profile.SolutionName}");
		if (!string.IsNullOrWhiteSpace(profile.AssemblyPath))
			Console.WriteLine($"  Assembly Path: {profile.AssemblyPath}");
		Console.WriteLine();

		// Display and validate sync items
		var allValid = true;
		if (profile.Sync.Count == 0)
		{
			Console.WriteLine("  ⊘ No sync items configured");
			Console.WriteLine();
		}
		else
		{
			Console.WriteLine($"  Sync Items ({profile.Sync.Count}):");
			Console.WriteLine();

			for (int i = 0; i < profile.Sync.Count; i++)
			{
				var syncItem = profile.Sync[i];
				allValid &= OutputSyncItemValidation(i + 1, syncItem, profile);
			}
		}

		// Display available commands
		var availableCommands = GetAvailableCommands(profile);
		if (availableCommands.Count != 0)
		{
			Console.WriteLine($"Available Commands: {string.Join(", ", availableCommands)}");
			Console.WriteLine();
		}

		return allValid;
	}

	private bool OutputSyncItemValidation(int index, SyncItem syncItem, ProfileConfiguration profile)
	{
		var itemLabel = $"  [{index}] {syncItem.SyncType}";

		try
		{
			var errors = syncItem switch
			{
				PluginSyncItem plugin => ValidatePluginSync(plugin, profile),
				PluginAnalysisSyncItem analysis => ValidatePluginAnalysis(analysis, profile),
				WebresourceSyncItem webresource => ValidateWebresource(webresource, profile),
				IdentitySyncItem identity => ValidateIdentity(identity, profile),
				_ => new List<string> { "Unknown sync item type" }
			};

			if (errors.Count > 0)
			{
				Console.WriteLine($"    ✗ {itemLabel}");
				DisplaySyncItemDetails(syncItem, profile);
				foreach (var error in errors)
				{
					Console.WriteLine($"      Error: {error}");
				}
				Console.WriteLine();
				return false;
			}

			Console.WriteLine($"    ✓ {itemLabel}");
			DisplaySyncItemDetails(syncItem, profile);
			Console.WriteLine();
			return true;
		}
		catch (Exception ex)
		{
			Console.WriteLine($"    ✗ {itemLabel}");
			Console.WriteLine($"      Error: {ex.Message}");
			Console.WriteLine();
			return false;
		}
	}

	private void DisplaySyncItemDetails(SyncItem syncItem, ProfileConfiguration profile)
	{
		switch (syncItem)
		{
			case PluginSyncItem plugin:
				Console.WriteLine($"      Assembly Path: {profile.ResolveAssemblyPath(plugin.AssemblyPath)}");
				Console.WriteLine($"      Solution Name: {profile.ResolveSolutionName(plugin)}");
				Console.WriteLine($"      Allow Empty Types: {plugin.AllowEmptyTypes}");
				Console.WriteLine($"      No Delete: {plugin.NoDelete}");
				Console.WriteLine($"      Watch: {plugin.Watch}");
				break;
			case PluginAnalysisSyncItem analysis:
				Console.WriteLine($"      Assembly Path: {profile.ResolveAssemblyPath(analysis.AssemblyPath)}");
				Console.WriteLine($"      Publisher Prefix: {analysis.PublisherPrefix}");
				Console.WriteLine($"      Pretty Print: {analysis.PrettyPrint}");
				break;
			case WebresourceSyncItem webresource:
				Console.WriteLine($"      Folder Path: {webresource.FolderPath}");
				Console.WriteLine($"      Solution Name: {profile.ResolveSolutionName(webresource)}");
				if (webresource.FileExtensions is { Count: > 0 })
					Console.WriteLine($"      File Extensions: {string.Join(", ", webresource.FileExtensions)}");
				Console.WriteLine($"      No Delete: {webresource.NoDelete}");
				Console.WriteLine($"      Watch: {webresource.Watch}");
				break;
			case IdentitySyncItem identity:
				Console.WriteLine($"      Operation: {identity.Operation}");
				Console.WriteLine($"      Assembly Path: {profile.ResolveAssemblyPath(identity.AssemblyPath)}");
				Console.WriteLine($"      Solution Name: {profile.ResolveSolutionName(identity)}");
				if (identity.Operation == IdentityOperation.Ensure)
				{
					Console.WriteLine($"      Client ID: {identity.ClientId}");
					Console.WriteLine($"      Tenant ID: {identity.TenantId}");
				}
				break;
		}
	}

	private static List<string> ValidatePluginSync(PluginSyncItem plugin, ProfileConfiguration profile) =>
	[
		.. XrmSyncConfigurationValidator.ValidateAssemblyPath(profile.ResolveAssemblyPath(plugin.AssemblyPath) ?? string.Empty),
		.. XrmSyncConfigurationValidator.ValidateSolutionName(profile.ResolveSolutionName(plugin))
	];

	private static List<string> ValidatePluginAnalysis(PluginAnalysisSyncItem analysis, ProfileConfiguration profile) =>
	[
		.. XrmSyncConfigurationValidator.ValidateAssemblyPath(profile.ResolveAssemblyPath(analysis.AssemblyPath) ?? string.Empty),
		.. XrmSyncConfigurationValidator.ValidatePublisherPrefix(analysis.PublisherPrefix)
	];

	private static List<string> ValidateWebresource(WebresourceSyncItem webresource, ProfileConfiguration profile) =>
	[
		.. XrmSyncConfigurationValidator.ValidateFolderPath(webresource.FolderPath),
		.. XrmSyncConfigurationValidator.ValidateSolutionName(profile.ResolveSolutionName(webresource))
	];

	private static List<string> ValidateIdentity(IdentitySyncItem identity, ProfileConfiguration profile)
	{
		var errors = new List<string>(XrmSyncConfigurationValidator.ValidateAssemblyPath(profile.ResolveAssemblyPath(identity.AssemblyPath) ?? string.Empty));
		errors.AddRange(XrmSyncConfigurationValidator.ValidateSolutionName(profile.ResolveSolutionName(identity)));

		if (identity.Operation == IdentityOperation.Ensure)
		{
			errors.AddRange(XrmSyncConfigurationValidator.ValidateGuid(identity.ClientId ?? string.Empty, "Client ID"));
			errors.AddRange(XrmSyncConfigurationValidator.ValidateGuid(identity.TenantId ?? string.Empty, "Tenant ID"));
		}

		return errors;
	}

	private List<string> GetAvailableCommands(ProfileConfiguration profile)
	{
		var commands = new List<string>();

		foreach (var syncItem in profile.Sync)
		{
			switch (syncItem)
			{
				case PluginSyncItem:
					if (!commands.Contains("plugins"))
						commands.Add("plugins");
					break;
				case PluginAnalysisSyncItem:
					if (!commands.Contains("analyze"))
						commands.Add("analyze");
					break;
				case WebresourceSyncItem:
					if (!commands.Contains("webresources"))
						commands.Add("webresources");
					break;
				case IdentitySyncItem:
					if (!commands.Contains("identity"))
						commands.Add("identity");
					break;
			}
		}

		return commands;
	}

	private static string GetConfigurationSource()
	{
		var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
		var baseFile = $"{ConfigReader.CONFIG_FILE_BASE}.json";
		var envFile = $"{ConfigReader.CONFIG_FILE_BASE}.{environment}.json";

		var basePath = Directory.GetCurrentDirectory();
		var baseExists = File.Exists(Path.Combine(basePath, baseFile));
		var envExists = File.Exists(Path.Combine(basePath, envFile));

		if (envExists && baseExists)
		{
			return $"{baseFile}, {envFile}";
		}
		else if (envExists)
		{
			return envFile;
		}
		else if (baseExists)
		{
			return baseFile;
		}
		else
		{
			return "no configuration file found";
		}
	}
}
