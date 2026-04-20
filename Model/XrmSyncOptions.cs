using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace XrmSync.Model;

public record XrmSyncConfiguration(bool DryRun, LogLevel LogLevel, bool CiMode, List<ProfileConfiguration> Profiles)
{
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

public record ProfileConfiguration(string Name, string SolutionName, List<SyncItem> Sync)
{
	public static ProfileConfiguration Empty => new(string.Empty, string.Empty, []);
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
}

public record PluginSyncItem(string AssemblyPath) : SyncItem
{
	public const string TypeName = "Plugin";
	public static PluginSyncItem Empty => new(string.Empty);

	[JsonIgnore]
	public override string SyncType => TypeName;
}

public record PluginAnalysisSyncItem(string AssemblyPath, string PublisherPrefix, bool PrettyPrint) : SyncItem
{
	public const string TypeName = "PluginAnalysis";
	public static PluginAnalysisSyncItem Empty => new(string.Empty, "new", false);

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

public record SharedOptions(string? ProfileName)
{
	public static SharedOptions Empty => new((string?)null);
}

// Command-specific options that can be populated from CLI or profile
public record PluginSyncCommandOptions(string AssemblyPath, string SolutionName)
{
	public static PluginSyncCommandOptions Empty => new(string.Empty, string.Empty);
}

public record PluginAnalysisCommandOptions(string AssemblyPath, string PublisherPrefix, bool PrettyPrint)
{
	public static PluginAnalysisCommandOptions Empty => new(string.Empty, "new", false);
}

public record WebresourceSyncCommandOptions(string FolderPath, string SolutionName, List<string>? FileExtensions = null)
{
	public static WebresourceSyncCommandOptions Empty => new(string.Empty, string.Empty);
}

public enum IdentityOperation
{
	Remove,
	Ensure
}

public record IdentitySyncItem(IdentityOperation Operation, string AssemblyPath, string? ClientId = null, string? TenantId = null) : SyncItem
{
	public const string TypeName = "Identity";
	public static IdentitySyncItem Empty => new(IdentityOperation.Remove, string.Empty);

	[JsonIgnore]
	public override string SyncType => $"{TypeName} ({Operation})";
}

public record IdentityCommandOptions(IdentityOperation Operation, string AssemblyPath, string SolutionName, string? ClientId = null, string? TenantId = null)
{
	public static IdentityCommandOptions Empty => new(IdentityOperation.Remove, string.Empty, string.Empty);
}

public record ExecutionModeOptions(bool DryRun)
{
	public static ExecutionModeOptions Empty => new(false);
}
