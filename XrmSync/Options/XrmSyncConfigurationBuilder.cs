using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XrmSync.Model;
using XrmSync.Model.Exceptions;

namespace XrmSync.Options;

internal class XrmSyncConfigurationBuilder(IConfiguration configuration) : IConfigurationBuilder
{

	public static class SectionName
	{
		public const string XrmSync = nameof(XrmSync);
		public const string Profiles = nameof(Profiles);
		public const string DryRun = nameof(DryRun);
		public const string LogLevel = nameof(LogLevel);
		public const string CiMode = nameof(CiMode);
	}

	public XrmSyncConfiguration Build()
	{
		var xrmSyncSection = configuration.GetSection(SectionName.XrmSync);

		return new XrmSyncConfiguration(
			xrmSyncSection.GetValue<bool>(SectionName.DryRun),
			xrmSyncSection.GetValue<LogLevel?>(SectionName.LogLevel) ?? LogLevel.Information,
			xrmSyncSection.GetValue<bool>(SectionName.CiMode),
			BuildProfiles(xrmSyncSection)
		);
	}

	private List<ProfileConfiguration> BuildProfiles(IConfigurationSection xrmSyncSection)
	{
		var profilesSection = xrmSyncSection.GetSection(SectionName.Profiles);

		if (!profilesSection.Exists())
		{
			return new List<ProfileConfiguration>();
		}

		var profiles = new List<ProfileConfiguration>();

		foreach (var profileSection in profilesSection.GetChildren())
		{
			var name = profileSection.GetValue<string>(nameof(ProfileConfiguration.Name)) ?? string.Empty;
			var solutionName = profileSection.GetValue<string>(nameof(ProfileConfiguration.SolutionName)) ?? string.Empty;
			var syncItems = BuildSyncItems(profileSection.GetSection(nameof(ProfileConfiguration.Sync)));

			profiles.Add(new ProfileConfiguration(name, solutionName, syncItems));
		}

		return profiles;
	}

	private List<SyncItem> BuildSyncItems(IConfigurationSection syncSection)
	{
		var syncItems = new List<SyncItem>();

		if (!syncSection.Exists())
		{
			return syncItems;
		}

		foreach (var itemSection in syncSection.GetChildren())
		{
			var type = itemSection.GetValue<string>("Type") ?? string.Empty;

			SyncItem? syncItem = type switch
			{
				"Plugin" => new PluginSyncItem(
					itemSection.GetValue<string>(nameof(PluginSyncItem.AssemblyPath)) ?? string.Empty
				),
				"PluginAnalysis" => new PluginAnalysisSyncItem(
					itemSection.GetValue<string>(nameof(PluginAnalysisSyncItem.AssemblyPath)) ?? string.Empty,
					itemSection.GetValue<string>(nameof(PluginAnalysisSyncItem.PublisherPrefix)) ?? "new",
					itemSection.GetValue<bool>(nameof(PluginAnalysisSyncItem.PrettyPrint))
				),
				"Webresource" => new WebresourceSyncItem(
					itemSection.GetValue<string>(nameof(WebresourceSyncItem.FolderPath)) ?? string.Empty
				),
				_ => null
			};

			if (syncItem != null)
			{
				syncItems.Add(syncItem);
			}
		}

		return syncItems;
	}

	public ProfileConfiguration? GetProfile(string? profileName)
	{
		var config = Build();
		var resolvedProfileName = ResolveProfileName(profileName, config.Profiles);

		return config.Profiles.FirstOrDefault(p => p.Name.Equals(resolvedProfileName, StringComparison.OrdinalIgnoreCase));
	}

	private static string? ResolveProfileName(string? requestedName, List<ProfileConfiguration> profiles)
	{
		if (profiles.Count == 0)
		{
			return null;
		}

		// If no name requested, fall back to "default"
		var effectiveName = requestedName ?? "default";

		// If effective name matches a profile, use it
		if (profiles.Any(p => p.Name.Equals(effectiveName, StringComparison.OrdinalIgnoreCase)))
		{
			return effectiveName;
		}

		// If only one profile exists, use it automatically
		if (profiles.Count == 1)
		{
			return profiles[0].Name;
		}

		// Multiple profiles, no match
		throw new XrmSyncException("Multiple profiles found. Use --profile to specify which profile to use, name a profile 'default', or run 'xrmsync config list' to see available profiles.");
	}
}
