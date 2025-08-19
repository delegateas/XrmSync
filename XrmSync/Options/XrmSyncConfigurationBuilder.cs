using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XrmSync.Model;

namespace XrmSync.Options;

internal class XrmSyncConfigurationBuilder(IConfiguration configuration) : IConfigurationBuilder
{
    public XrmSyncConfiguration Build()
    {
        return new XrmSyncConfiguration(new PluginOptions(
            BuildPluginSyncOptions(),
            BuildAnalyzisOptions()
        ));
    }

    private PluginSyncOptions BuildPluginSyncOptions()
    {
        var pluginSyncSection = configuration.GetSection("XrmSync:Plugin:Sync");
        return new PluginSyncOptions(
            pluginSyncSection.GetValue<string>(nameof(PluginSyncOptions.AssemblyPath)) ?? string.Empty,
            pluginSyncSection.GetValue<string>(nameof(PluginSyncOptions.SolutionName)) ?? string.Empty,
            pluginSyncSection.GetValue<LogLevel?>(nameof(PluginSyncOptions.LogLevel)) ?? LogLevel.Information,
            pluginSyncSection.GetValue<bool>(nameof(PluginSyncOptions.DryRun))
        );
    }

    public PluginAnalysisOptions BuildAnalyzisOptions()
    {
        var analysisSection = configuration.GetSection("XrmSync:Plugin:Analysis");
        return new PluginAnalysisOptions(
            analysisSection.GetValue<string>(nameof(PluginAnalysisOptions.AssemblyPath)) ?? string.Empty,
            analysisSection.GetValue<string>(nameof(PluginAnalysisOptions.PublisherPrefix)) ?? "new",
            analysisSection.GetValue<bool>(nameof(PluginAnalysisOptions.PrettyPrint))
        );
    }
}
