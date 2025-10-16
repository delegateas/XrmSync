using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace XrmSync.Model;

public record XrmSyncConfiguration(PluginOptions Plugin, WebresourceOptions Webresource, LoggerOptions Logger)
{
    public static XrmSyncConfiguration Empty => new (PluginOptions.Empty, WebresourceOptions.Empty, LoggerOptions.Empty);
}
public record PluginOptions(PluginSyncOptions Sync, PluginAnalysisOptions Analysis)
{
    public static PluginOptions Empty => new(PluginSyncOptions.Empty, PluginAnalysisOptions.Empty);
}

public record WebresourceOptions(WebresourceSyncOptions Sync)
{
    public static WebresourceOptions Empty => new (WebresourceSyncOptions.Empty);
}

public record PluginSyncOptions(string AssemblyPath, string SolutionName, bool DryRun)
{
    public static PluginSyncOptions Empty => new (string.Empty, string.Empty, false);
}

public record PluginAnalysisOptions(string AssemblyPath, string PublisherPrefix, bool PrettyPrint)
{
    public static PluginAnalysisOptions Empty => new (string.Empty, "new", false);
}

public record WebresourceSyncOptions(string FolderPath, string SolutionName, bool DryRun)
{
    public static WebresourceSyncOptions Empty => new (string.Empty, string.Empty, false);
}

public record LoggerOptions(LogLevel LogLevel, bool CiMode)
{
    public static LoggerOptions Empty => new (LogLevel.Information, false);
}