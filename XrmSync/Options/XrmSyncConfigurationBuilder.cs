using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XrmSync.Model;
using XrmSync.Model.Exceptions;

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
				PluginSyncItem.TypeName => new PluginSyncItem(
					itemSection.GetValue<string>(nameof(PluginSyncItem.AssemblyPath)) ?? string.Empty
				),
				PluginAnalysisSyncItem.TypeName => new PluginAnalysisSyncItem(
					itemSection.GetValue<string>(nameof(PluginAnalysisSyncItem.AssemblyPath)) ?? string.Empty,
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
				syncItems.Add(syncItem);
			}
		}

		return syncItems;
	}

	private static IdentitySyncItem? BuildIdentitySyncItem(IConfigurationSection itemSection)
	{
		var operationStr = itemSection.GetValue<string>(nameof(IdentitySyncItem.Operation)) ?? string.Empty;
		if (!Enum.TryParse<IdentityOperation>(operationStr, ignoreCase: true, out var operation))
		{
			return null;
		}

		return new IdentitySyncItem(
			operation,
			itemSection.GetValue<string>(nameof(IdentitySyncItem.AssemblyPath)) ?? string.Empty,
			itemSection.GetValue<string>(nameof(IdentitySyncItem.ClientId)),
			itemSection.GetValue<string>(nameof(IdentitySyncItem.TenantId))
		);
	}

	public ProfileConfiguration? GetProfile(string? profileName)
	{
		return Build().ResolveProfile(profileName);
	}
}
