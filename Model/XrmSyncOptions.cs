using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace XrmSync.Model;

public record XrmSyncConfiguration(bool DryRun, LogLevel LogLevel, bool CiMode, List<ProfileConfiguration> Profiles, int WatchDebounceMs = XrmSyncConfiguration.DefaultWatchDebounceMs)
{
	/// <summary>
	/// Quiet period, in milliseconds, that watch mode waits for after a file change before re-syncing.
	/// </summary>
	public const int DefaultWatchDebounceMs = 500;

	public static XrmSyncConfiguration Empty => new(false, LogLevel.Information, false, []);

	/// <summary>
	/// Resolves the effective profile using fallback logic:
	/// 1. If a name is requested and matches, use it
	/// 2. If no name is requested, fall back to "default"
	/// 3. If only one profile exists, use it automatically
	/// Returns null when no profiles are configured.
	/// </summary>
	public ProfileConfiguration? ResolveProfile(string? requestedName)
	{
		if (Profiles.Count == 0)
		{
			return requestedName != null
				? throw new Exceptions.XrmSyncException($"Profile '{requestedName}' not found. No profiles are configured.")
				: null;
		}

		// Explicit profile name requested — must match exactly
		if (requestedName != null)
		{
			return Profiles.FirstOrDefault(p => p.Name.Equals(requestedName, StringComparison.OrdinalIgnoreCase))
				?? throw new Exceptions.XrmSyncException($"Profile '{requestedName}' not found. Available profiles: {string.Join(", ", Profiles.Select(p => p.Name))}");
		}

		// No name specified — try "default", then single-profile auto-select
		var defaultProfile = Profiles.FirstOrDefault(p => p.Name.Equals("default", StringComparison.OrdinalIgnoreCase));
		if (defaultProfile != null)
		{
			return defaultProfile;
		}

		if (Profiles.Count == 1)
		{
			return Profiles[0];
		}

		throw new Exceptions.XrmSyncException("Multiple profiles found. Use --profile to specify which profile to use, name a profile 'default', or run 'xrmsync config list' to see available profiles.");
	}
}

public record ProfileConfiguration(string Name, string SolutionName, List<SyncItem> Sync, string? AssemblyPath = null)
{
	public static ProfileConfiguration Empty => new(string.Empty, string.Empty, []);

	/// <summary>
	/// Resolves the effective solution name for a sync item: a per-item override takes precedence,
	/// falling back to the profile-level <see cref="SolutionName"/> when none is specified on the item.
	/// </summary>
	public string ResolveSolutionName(SyncItem? item) =>
		string.IsNullOrWhiteSpace(item?.SolutionName) ? SolutionName : item.SolutionName;

	/// <summary>
	/// Resolves the effective assembly path: a per-item value takes precedence, falling back to the
	/// shared profile-level <see cref="AssemblyPath"/> when the item does not specify its own.
	/// Returns null when neither level provides a value.
	/// </summary>
	public string? ResolveAssemblyPath(string? itemAssemblyPath) =>
		!string.IsNullOrWhiteSpace(itemAssemblyPath) ? itemAssemblyPath
		: !string.IsNullOrWhiteSpace(AssemblyPath) ? AssemblyPath
		: null;
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(PluginSyncItem), typeDiscriminator: PluginSyncItem.TypeName)]
[JsonDerivedType(typeof(PluginAnalysisSyncItem), typeDiscriminator: PluginAnalysisSyncItem.TypeName)]
[JsonDerivedType(typeof(WebresourceSyncItem), typeDiscriminator: WebresourceSyncItem.TypeName)]
[JsonDerivedType(typeof(IdentitySyncItem), typeDiscriminator: IdentitySyncItem.TypeName)]
public abstract record SyncItem
{
	[JsonIgnore]
	public abstract string SyncType { get; }

	/// <summary>
	/// Optional per-item solution name override. When null, the profile-level SolutionName is used.
	/// </summary>
	public string? SolutionName { get; init; }

	/// <summary>
	/// When true, the item is watched and re-synced automatically whenever its input changes
	/// (the plugin assembly, or any supported file under the webresource folder).
	/// Only Plugin and Webresource items are watchable; the flag is ignored on other types.
	/// </summary>
	public bool Watch { get; init; }
}

public record PluginSyncItem(string? AssemblyPath = null, string? ManagedIdentityClientId = null, string? ManagedIdentityTenantId = null, bool AllowEmptyTypes = false) : SyncItem
{
	public const string TypeName = "Plugin";
	public static PluginSyncItem Empty => new();

	[JsonIgnore]
	public override string SyncType => TypeName;
}

public record PluginAnalysisSyncItem(string? AssemblyPath, string PublisherPrefix, bool PrettyPrint) : SyncItem
{
	public const string TypeName = "PluginAnalysis";
	public static PluginAnalysisSyncItem Empty => new(null, "new", false);

	[JsonIgnore]
	public override string SyncType => TypeName;
}

public record WebresourceSyncItem(string FolderPath, List<string>? FileExtensions = null) : SyncItem
{
	public const string TypeName = "Webresource";
	public static WebresourceSyncItem Empty => new(string.Empty);

	[JsonIgnore]
	public override string SyncType => TypeName;
}

public enum IdentityOperation
{
	Remove,
	Ensure
}

public record IdentitySyncItem(IdentityOperation? Operation = null, string? AssemblyPath = null, string ClientId = "", string TenantId = "") : SyncItem
{
	public const string TypeName = "Identity";
	public static IdentitySyncItem Empty => new();

	[JsonIgnore]
	public override string SyncType => Operation.HasValue ? $"{TypeName} ({Operation})" : TypeName;
}

