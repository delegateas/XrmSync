using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XrmSync.Model;

namespace XrmSync.Options;

internal class XrmSyncConfigurationBuilder(IConfiguration configuration, string? configName = null) : IConfigurationBuilder
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
        var pluginSyncSection = GetConfigurationSection("Plugin:Sync");
        return new PluginSyncOptions(
            pluginSyncSection.GetValue<string>(nameof(PluginSyncOptions.AssemblyPath)) ?? string.Empty,
            pluginSyncSection.GetValue<string>(nameof(PluginSyncOptions.SolutionName)) ?? string.Empty,
            pluginSyncSection.GetValue<LogLevel?>(nameof(PluginSyncOptions.LogLevel)) ?? LogLevel.Information,
            pluginSyncSection.GetValue<bool>(nameof(PluginSyncOptions.DryRun))
        );
    }

    public PluginAnalysisOptions BuildAnalyzisOptions()
    {
        var analysisSection = GetConfigurationSection("Plugin:Analysis");
        return new PluginAnalysisOptions(
            analysisSection.GetValue<string>(nameof(PluginAnalysisOptions.AssemblyPath)) ?? string.Empty,
            analysisSection.GetValue<string>(nameof(PluginAnalysisOptions.PublisherPrefix)) ?? "new",
            analysisSection.GetValue<bool>(nameof(PluginAnalysisOptions.PrettyPrint))
        );
    }

    private IConfigurationSection GetConfigurationSection(string sectionPath)
    {
        // If a config name is specified, use named configuration
        if (!string.IsNullOrWhiteSpace(configName))
        {
            return configuration.GetSection($"XrmSync:{configName}:{sectionPath}");
        }

        // Otherwise, use legacy structure
        return configuration.GetSection($"XrmSync:{sectionPath}");
    }
}
