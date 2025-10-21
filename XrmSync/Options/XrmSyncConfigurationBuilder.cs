using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XrmSync.Commands;
using XrmSync.Model;
using XrmSync.Model.Exceptions;

namespace XrmSync.Options;

internal class XrmSyncConfigurationBuilder(IConfiguration configuration, IOptions<SharedOptions> options) : IConfigurationBuilder
{
    internal const string DEFAULT_CONFIG_NAME = "default";

    public static class SectionName
    {
        public const string XrmSync = nameof(XrmSync);
        public const string Plugin = nameof(Plugin);
        public const string Webresource = nameof(Webresource);
        public const string Sync = nameof(Sync);
        public const string Analysis = nameof(Analysis);
        public const string Logger = nameof(Logger);
        public const string Execution = nameof(Execution);
    }

    public XrmSyncConfiguration Build()
    {
        return new XrmSyncConfiguration(
            new(
                BuildPluginSyncOptions(),
                BuildAnalysisOptions()
            ),
            new(
                BuildWebresourceSyncOptions()
            ),
            BuildLoggerOptions(),
            BuildExecutionOptions()
        );
    }

    private PluginSyncOptions BuildPluginSyncOptions()
    {
        var pluginSyncSection = GetConfigurationSection($"{SectionName.Plugin}:{SectionName.Sync}");
        return new PluginSyncOptions(
            pluginSyncSection.GetValue<string>(nameof(PluginSyncOptions.AssemblyPath)) ?? string.Empty,
            pluginSyncSection.GetValue<string>(nameof(PluginSyncOptions.SolutionName)) ?? string.Empty
        );
    }

    private WebresourceSyncOptions BuildWebresourceSyncOptions()
    {
        var webresourceSyncSection = GetConfigurationSection($"{SectionName.Webresource}:{SectionName.Sync}");
        return new WebresourceSyncOptions(
            webresourceSyncSection.GetValue<string>(nameof(WebresourceSyncOptions.FolderPath)) ?? string.Empty,
            webresourceSyncSection.GetValue<string>(nameof(WebresourceSyncOptions.SolutionName)) ?? string.Empty
        );
    }

    private PluginAnalysisOptions BuildAnalysisOptions()
    {
        var analysisSection = GetConfigurationSection($"{SectionName.Plugin}:{SectionName.Analysis}");
        return new (
            analysisSection.GetValue<string>(nameof(PluginAnalysisOptions.AssemblyPath)) ?? string.Empty,
            analysisSection.GetValue<string>(nameof(PluginAnalysisOptions.PublisherPrefix)) ?? "new",
            analysisSection.GetValue<bool>(nameof(PluginAnalysisOptions.PrettyPrint))
        );
    }

    private LoggerOptions BuildLoggerOptions()
    {
        var loggerSection = GetConfigurationSection(SectionName.Logger);
        return new (
            loggerSection.GetValue<LogLevel?>(nameof(LoggerOptions.LogLevel)) ?? LogLevel.Information,
            loggerSection.GetValue<bool>(nameof(LoggerOptions.CiMode))
        );
    }

    private ExecutionOptions BuildExecutionOptions()
    {
        var executionSection = GetConfigurationSection(SectionName.Execution);
        return new (
            executionSection.GetValue<bool>(nameof(ExecutionOptions.DryRun))
        );
    }

    private IConfigurationSection GetConfigurationSection(string sectionPath)
    {
        // If a config name is specified, use named configuration
        var resolvedConfigName = ResolveConfigurationName(options.Value.ConfigName);
        
        return configuration.GetSection($"{SectionName.XrmSync}:{resolvedConfigName}:{sectionPath}");
    }

    private string ResolveConfigurationName(string requestedName)
    {
        var xrmSyncSection = configuration.GetSection(SectionName.XrmSync);

        if (!xrmSyncSection.Exists())
        {
            return DEFAULT_CONFIG_NAME;
        }

        // Get all configuration names (direct children of XrmSync)
        var configNames = xrmSyncSection.GetChildren()
            .Select(c => c.Key)
            .ToList();

        // If requested name exists, use it
        if (configNames.Contains(requestedName))
        {
            return requestedName;
        }
        // If only one named config exists, use it
        if (configNames.Count == 1)
        {
            return configNames[0];
        }

        // If multiple configs exist, try to use "default"
        if (configNames.Contains(DEFAULT_CONFIG_NAME))
        {
            return DEFAULT_CONFIG_NAME;
        }

        // Throw exception if we cannot resolve the configuration
        throw new XrmSyncException($"Multiple configuration sections found under '{SectionName.XrmSync}', but no {DEFAULT_CONFIG_NAME} configuration section found, or name wasn't specified.");
    }
}
