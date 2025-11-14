using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XrmSync.Model;
using XrmSync.Model.Exceptions;

namespace XrmSync.Options;

#pragma warning disable CS9113 // Parameter is unread - false positive, used in GetProfile method
internal class XrmSyncConfigurationBuilder(IConfiguration configuration, IOptions<SharedOptions> options) : IConfigurationBuilder
#pragma warning restore CS9113
{
    internal const string DEFAULT_PROFILE_NAME = "default";

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
            xrmSyncSection.GetValue<LogLevel?>(SectionName.LogLevel) ?? Microsoft.Extensions.Logging.LogLevel.Information,
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

    private string ResolveProfileName(string? requestedName, List<ProfileConfiguration> profiles)
    {
        if (profiles.Count == 0)
        {
            return DEFAULT_PROFILE_NAME;
        }

        // If requested name exists, use it
        if (!string.IsNullOrWhiteSpace(requestedName) && profiles.Any(p => p.Name.Equals(requestedName, StringComparison.OrdinalIgnoreCase)))
        {
            return requestedName;
        }

        // If only one profile exists, use it
        if (profiles.Count == 1)
        {
            return profiles[0].Name;
        }

        // If multiple profiles exist, try to use "default"
        var defaultProfile = profiles.FirstOrDefault(p => p.Name.Equals(DEFAULT_PROFILE_NAME, StringComparison.OrdinalIgnoreCase));
        if (defaultProfile != null)
        {
            return defaultProfile.Name;
        }

        // Throw exception if we cannot resolve the profile
        throw new XrmSyncException($"Multiple profiles found, but no '{DEFAULT_PROFILE_NAME}' profile found, and no profile name was specified. Use --profile to specify which profile to use.");
    }
}
