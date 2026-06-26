using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XrmSync.Model;

namespace XrmSync.Options;

internal class XrmSyncConfigurationBuilder(IConfiguration configuration) : IConfigurationBuilder
{
	private XrmSyncConfiguration? cachedConfiguration;

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
		if (cachedConfiguration != null)
		{
			return cachedConfiguration;
		}

		var xrmSyncSection = configuration.GetSection(SectionName.XrmSync);

		cachedConfiguration = new XrmSyncConfiguration(
			xrmSyncSection.GetValue<bool>(SectionName.DryRun),
			xrmSyncSection.GetValue<LogLevel?>(SectionName.LogLevel) ?? LogLevel.Information,
			xrmSyncSection.GetValue<bool>(SectionName.CiMode),
			BuildProfiles(xrmSyncSection)
		);

		return cachedConfiguration;
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
			var assemblyPath = profileSection.GetValue<string>(nameof(ProfileConfiguration.AssemblyPath));
			var syncItems = BuildSyncItems(profileSection.GetSection(nameof(ProfileConfiguration.Sync)));

			profiles.Add(new ProfileConfiguration(name, solutionName, syncItems, assemblyPath));
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
				PluginSyncItem.TypeName => new PluginSyncItem(
					itemSection.GetValue<string>(nameof(PluginSyncItem.AssemblyPath)),
					itemSection.GetValue<string>(nameof(PluginSyncItem.ManagedIdentityClientId)),
					itemSection.GetValue<string>(nameof(PluginSyncItem.ManagedIdentityTenantId))
				),
				PluginAnalysisSyncItem.TypeName => new PluginAnalysisSyncItem(
					itemSection.GetValue<string>(nameof(PluginAnalysisSyncItem.AssemblyPath)),
					itemSection.GetValue<string>(nameof(PluginAnalysisSyncItem.PublisherPrefix)) ?? "new",
					itemSection.GetValue<bool>(nameof(PluginAnalysisSyncItem.PrettyPrint))
				),
				WebresourceSyncItem.TypeName => new WebresourceSyncItem(
					itemSection.GetValue<string>(nameof(WebresourceSyncItem.FolderPath)) ?? string.Empty,
					itemSection.GetSection(nameof(WebresourceSyncItem.FileExtensions)).Get<List<string>>()
				),
				IdentitySyncItem.TypeName => BuildIdentitySyncItem(itemSection),
				_ => null
			};

			if (syncItem != null)
			{
				// Optional per-item solution name override (falls back to the profile-level value)
				var itemSolutionName = itemSection.GetValue<string>(nameof(SyncItem.SolutionName));
				if (!string.IsNullOrWhiteSpace(itemSolutionName))
				{
					syncItem = syncItem with { SolutionName = itemSolutionName };
				}

				syncItems.Add(syncItem);
			}
		}

		return syncItems;
	}

	private static IdentitySyncItem? BuildIdentitySyncItem(IConfigurationSection itemSection)
	{
		var operationStr = itemSection.GetValue<string>(nameof(IdentitySyncItem.Operation));
		IdentityOperation? operation = null;
		if (operationStr != null)
		{
			if (!Enum.TryParse<IdentityOperation>(operationStr, ignoreCase: true, out var parsed))
				return null; // Invalid string → skip as before
			operation = parsed;
		}
		// operation == null → absent from config, to be supplied via CLI

		return new IdentitySyncItem(
			operation,
			itemSection.GetValue<string>(nameof(IdentitySyncItem.AssemblyPath)),
			itemSection.GetValue<string>(nameof(IdentitySyncItem.ClientId)) ?? string.Empty,
			itemSection.GetValue<string>(nameof(IdentitySyncItem.TenantId)) ?? string.Empty
		);
	}

	public ProfileConfiguration? GetProfile(string? profileName)
	{
		return Build().ResolveProfile(profileName);
	}
}
