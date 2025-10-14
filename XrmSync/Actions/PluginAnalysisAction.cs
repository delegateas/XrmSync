using System.Text.Json;
using Microsoft.Extensions.Options;
using XrmSync.AssemblyAnalyzer;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Options;

namespace XrmSync.Actions;

internal class PluginAnalysisAction(IAssemblyAnalyzer analyzer, IOptions<XrmSyncConfiguration> configuration) : IAction
{
    public async Task<bool> RunAction(CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            try
            {
                var analysisOptions = configuration.Value.Plugin?.Analysis
                    ?? throw new XrmSyncException("No analysis configuration found in the plugin sync configuration.");

                var pluginDto = analyzer.AnalyzeAssembly(analysisOptions.AssemblyPath, analysisOptions.PublisherPrefix);
                var jsonOptions = new JsonSerializerOptions(JsonSerializerOptions.Default)
                {
                    WriteIndented = analysisOptions.PrettyPrint
                };

                var jsonOutput = JsonSerializer.Serialize(pluginDto, jsonOptions);
                Console.WriteLine(jsonOutput);
                return true;
            }
            catch (XrmSyncException ex)
            {
                Console.Error.WriteLine($"Error analyzing assembly: {ex.Message}");
                return false;
            }
        });
    }
}

internal class SavePluginAnalysisConfigAction(IOptions<XrmSyncConfiguration> config, IConfigWriter configWriter) : ISaveConfigAction
{
    public async Task<bool> SaveConfigAsync(string? filename, CancellationToken cancellationToken)
    {
        // Handle save-config functionality
        if (config.Value.Plugin?.Analysis is null)
        {
            throw new XrmSyncException(filename is null
                ? "No analysis configuration loaded - cannot save"
                : $"No analysis configuration loaded - cannot save to {filename}");
        }

        var configPath = string.IsNullOrWhiteSpace(filename) ? null : filename;
        await configWriter.SaveAnalysisConfigAsync(config.Value.Plugin.Analysis, configPath, cancellationToken);
        Console.WriteLine($"Configuration saved to {configPath ?? $"{ConfigReader.CONFIG_FILE_BASE}.json"}");
        return true;
    }
}
