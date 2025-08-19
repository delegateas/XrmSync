using Microsoft.Extensions.Logging;

namespace XrmSync.Model;

public record XrmSyncConfiguration(PluginOptions? Plugin);
public record PluginOptions(PluginSyncOptions? Sync, PluginAnalysisOptions? Analysis);
public record PluginSyncOptions(string AssemblyPath, string SolutionName, LogLevel LogLevel, bool DryRun);
public record PluginAnalysisOptions(string AssemblyPath, string PublisherPrefix, bool PrettyPrint);


