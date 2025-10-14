using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace XrmSync.Model;

public record XrmSyncConfiguration(PluginOptions? Plugin);
public record PluginOptions(PluginSyncOptions? Sync, PluginAnalysisOptions? Analysis);
public record PluginSyncOptions(string AssemblyPath, string SolutionName, LogLevel LogLevel, bool DryRun);
public record PluginAnalysisOptions(string AssemblyPath, string PublisherPrefix, bool PrettyPrint);


