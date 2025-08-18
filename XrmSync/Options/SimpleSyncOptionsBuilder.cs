using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XrmSync.Model;

namespace XrmSync.Options;

internal class SimpleSyncOptionsBuilder(IConfiguration configuration) : ISyncOptionsBuilder
{
    public XrmSyncOptions Build()
    {
        var pluginSyncSection = configuration.GetSection("XrmSync:Plugin:Sync");
        return new XrmSyncOptions(
            pluginSyncSection.GetValue<string>(nameof(XrmSyncOptions.AssemblyPath)) ?? string.Empty,
            pluginSyncSection.GetValue<string>(nameof(XrmSyncOptions.SolutionName)) ?? string.Empty,
            pluginSyncSection.GetValue<LogLevel?>(nameof(XrmSyncOptions.LogLevel)) ?? LogLevel.Information,
            pluginSyncSection.GetValue<bool>(nameof(XrmSyncOptions.DryRun))
        );
    }
}

internal class SimpleAnalysisOptionsBuilder(IConfiguration configuration) : IAnalysisOptionsBuilder
{
    public PluginAnalysisOptions Build()
    {
        var analysisSection = configuration.GetSection("XrmSync:Plugin:Analysis");
        return new PluginAnalysisOptions(
            analysisSection.GetValue<string>(nameof(PluginAnalysisOptions.AssemblyPath)) ?? string.Empty,
            analysisSection.GetValue<string>(nameof(PluginAnalysisOptions.PublisherPrefix)) ?? "new",
            analysisSection.GetValue<bool>(nameof(PluginAnalysisOptions.PrettyPrint))
        );
    }
}
