using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

[assembly: InternalsVisibleTo("Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace XrmSync.Model;

public record XrmSyncConfiguration(bool DryRun, LogLevel LogLevel, bool CiMode, List<ProfileConfiguration> Profiles)
{
	public static XrmSyncConfiguration Empty => new(false, LogLevel.Information, false, []);
}

public record ProfileConfiguration(string Name, string SolutionName, List<SyncItem> Sync)
{
	public static ProfileConfiguration Empty => new(string.Empty, string.Empty, []);
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(PluginSyncItem), typeDiscriminator: "Plugin")]
[JsonDerivedType(typeof(PluginAnalysisSyncItem), typeDiscriminator: "PluginAnalysis")]
[JsonDerivedType(typeof(WebresourceSyncItem), typeDiscriminator: "Webresource")]
public abstract record SyncItem
{
	[JsonIgnore]
	public abstract string SyncType { get; }
}

public record PluginSyncItem(string AssemblyPath) : SyncItem
{
	public static PluginSyncItem Empty => new(string.Empty);

	[JsonIgnore]
	public override string SyncType => "Plugin";
}

public record PluginAnalysisSyncItem(string AssemblyPath, string PublisherPrefix, bool PrettyPrint) : SyncItem
{
	public static PluginAnalysisSyncItem Empty => new(string.Empty, "new", false);

	[JsonIgnore]
	public override string SyncType => "PluginAnalysis";
}

public record WebresourceSyncItem(string FolderPath) : SyncItem
{
	public static WebresourceSyncItem Empty => new(string.Empty);

	[JsonIgnore]
	public override string SyncType => "Webresource";
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

public record WebresourceSyncCommandOptions(string FolderPath, string SolutionName)
{
	public static WebresourceSyncCommandOptions Empty => new(string.Empty, string.Empty);
}

public record ExecutionModeOptions(bool DryRun)
{
	public static ExecutionModeOptions Empty => new(false);
}
