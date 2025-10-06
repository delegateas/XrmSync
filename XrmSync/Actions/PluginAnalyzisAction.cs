using System.Text.Json;
using XrmSync.AssemblyAnalyzer;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Options;

namespace XrmSync.Actions;

internal class PluginAnalyzisAction(IAssemblyAnalyzer analyzer, XrmSyncConfiguration configuration) : IAction
{
    public async Task<bool> RunAction(CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            try
            {
                var analyzisOptions = configuration.Plugin?.Analysis
                    ?? throw new XrmSyncException("No analyzis configuration found in the plugin sync configuration.");

                var pluginDto = analyzer.AnalyzeAssembly(analyzisOptions.AssemblyPath, analyzisOptions.PublisherPrefix);
                var jsonOptions = new JsonSerializerOptions(JsonSerializerOptions.Default)
                {
                    WriteIndented = analyzisOptions.PrettyPrint
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

internal class SavePluginAnalyzisConfigAction(XrmSyncConfiguration config, IConfigWriter configWriter) : ISaveConfigAction
{
    public async Task<bool> SaveConfigAsync(string? filename, CancellationToken cancellationToken)
    {
        // Handle save-config functionality
        if (config.Plugin?.Analysis is null)
        {
            throw new XrmSyncException(filename is null
                ? "No analyzis configuration loaded - cannot save"
                : $"No analyzis configuration loaded - cannot save to {filename}");
        }

        var configPath = string.IsNullOrWhiteSpace(filename) ? null : filename;
        await configWriter.SaveAnalysisConfigAsync(config.Plugin.Analysis, configPath, cancellationToken);
        Console.WriteLine($"Configuration saved to {configPath ?? $"{ConfigReader.CONFIG_FILE_BASE}.json"}");
        return true;
    }
}
