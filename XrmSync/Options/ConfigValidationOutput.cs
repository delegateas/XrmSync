using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using XrmSync.Model;

namespace XrmSync.Options;

internal class ConfigValidationOutput(
	IConfiguration configuration,
	IOptions<XrmSyncConfiguration>? configOptions = null,
	IOptions<SharedOptions>? sharedOptions = null) : IConfigValidationOutput
{
	public Task OutputValidationResult(CancellationToken cancellationToken = default)
	{
		if (configOptions == null || sharedOptions == null)
		{
			throw new InvalidOperationException("ConfigValidationOutput requires XrmSyncConfiguration and SharedOptions to validate configuration. Use OutputConfigList for listing profiles.");
		}

		var profileName = sharedOptions.Value.ProfileName;
		var configSource = GetConfigurationSource();

		var config = configOptions.Value;
		var profile = config.Profiles.FirstOrDefault(p => p.Name.Equals(profileName, StringComparison.OrdinalIgnoreCase));

		if (profile == null)
		{
			Console.WriteLine($"Profile '{profileName}' not found in {configSource}");
			return Task.CompletedTask;
		}

		Console.WriteLine($"Profile: '{profile.Name}' (from {configSource})");
		Console.WriteLine();

		// Display global settings
		Console.WriteLine("✓ Global Configuration");
		Console.WriteLine($"  Dry Run: {config.DryRun}");
		Console.WriteLine($"  Log Level: {config.LogLevel}");
		Console.WriteLine($"  CI Mode: {config.CiMode}");
		Console.WriteLine();

		// Display profile settings
		Console.WriteLine($"✓ Profile '{profile.Name}'");
		Console.WriteLine($"  Solution Name: {profile.SolutionName}");
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
				allValid &= OutputSyncItemValidation(i + 1, syncItem, profile.Name);
			}
		}

		// Display available commands
		var availableCommands = GetAvailableCommands(profile);
		if (availableCommands.Count != 0)
		{
			Console.WriteLine($"Available Commands: {string.Join(", ", availableCommands)}");
			Console.WriteLine();
		}

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

	private bool OutputSyncItemValidation(int index, SyncItem syncItem, string profileName)
	{
		var itemLabel = $"  [{index}] {syncItem.SyncType}";

		try
		{
			var errors = syncItem switch
			{
				PluginSyncItem plugin => ValidatePluginSync(plugin),
				PluginAnalysisSyncItem analysis => ValidatePluginAnalysis(analysis),
				WebresourceSyncItem webresource => ValidateWebresource(webresource),
				_ => new List<string> { "Unknown sync item type" }
			};

			if (errors.Count > 0)
			{
				Console.WriteLine($"    ✗ {itemLabel}");
				DisplaySyncItemDetails(syncItem);
				foreach (var error in errors)
				{
					Console.WriteLine($"      Error: {error}");
				}
				Console.WriteLine();
				return false;
			}

			Console.WriteLine($"    ✓ {itemLabel}");
			DisplaySyncItemDetails(syncItem);
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

	private void DisplaySyncItemDetails(SyncItem syncItem)
	{
		switch (syncItem)
		{
			case PluginSyncItem plugin:
				Console.WriteLine($"      Assembly Path: {plugin.AssemblyPath}");
				break;
			case PluginAnalysisSyncItem analysis:
				Console.WriteLine($"      Assembly Path: {analysis.AssemblyPath}");
				Console.WriteLine($"      Publisher Prefix: {analysis.PublisherPrefix}");
				Console.WriteLine($"      Pretty Print: {analysis.PrettyPrint}");
				break;
			case WebresourceSyncItem webresource:
				Console.WriteLine($"      Folder Path: {webresource.FolderPath}");
				break;
		}
	}

	private List<string> ValidatePluginSync(PluginSyncItem plugin)
	{
		var errors = new List<string>();

		if (string.IsNullOrWhiteSpace(plugin.AssemblyPath))
		{
			errors.Add("Assembly path is required and cannot be empty.");
		}
		else if (!File.Exists(Path.GetFullPath(plugin.AssemblyPath)))
		{
			errors.Add($"Assembly file does not exist: {plugin.AssemblyPath}");
		}
		else if (!Path.GetExtension(plugin.AssemblyPath).Equals(".dll", StringComparison.OrdinalIgnoreCase))
		{
			errors.Add("Assembly file must have a .dll extension.");
		}

		return errors;
	}

	private List<string> ValidatePluginAnalysis(PluginAnalysisSyncItem analysis)
	{
		var errors = ValidatePluginSync(new PluginSyncItem(analysis.AssemblyPath));

		if (string.IsNullOrWhiteSpace(analysis.PublisherPrefix))
		{
			errors.Add("Publisher prefix is required and cannot be empty.");
		}
		else if (analysis.PublisherPrefix.Length < 2 || analysis.PublisherPrefix.Length > 8)
		{
			errors.Add("Publisher prefix must be between 2 and 8 characters.");
		}
		else if (!System.Text.RegularExpressions.Regex.IsMatch(analysis.PublisherPrefix, @"^[a-z][a-z0-9]{1,7}$"))
		{
			errors.Add("Publisher prefix must start with a lowercase letter and contain only lowercase letters and numbers.");
		}

		return errors;
	}

	private List<string> ValidateWebresource(WebresourceSyncItem webresource)
	{
		var errors = new List<string>();

		if (string.IsNullOrWhiteSpace(webresource.FolderPath))
		{
			errors.Add("Webresource root path is required and cannot be empty.");
		}
		else if (!Directory.Exists(Path.GetFullPath(webresource.FolderPath)))
		{
			errors.Add($"Webresource root path does not exist: {webresource.FolderPath}");
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
