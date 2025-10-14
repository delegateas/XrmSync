using System.Text.Json;
using Microsoft.Extensions.Options;
using XrmSync.AssemblyAnalyzer;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Options;

namespace XrmSync.Actions;

internal class PluginAnalysisAction(IAssemblyAnalyzer analyzer, IOptions<PluginAnalysisOptions> configuration) : IAction
{
    public async Task<bool> RunAction(CancellationToken cancellationToken)
    {
        return await Task.Run(() =>
        {
            try
            {
                var analysisOptions = configuration.Value;

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

internal class SavePluginAnalysisConfigAction(IOptions<PluginAnalysisOptions> config, IConfigWriter configWriter) : ISaveConfigAction
{
    public async Task<bool> SaveConfigAsync(string? filename, CancellationToken cancellationToken)
    {
        var configPath = string.IsNullOrWhiteSpace(filename) ? null : filename;
        await configWriter.SaveAnalysisConfigAsync(config.Value, configPath, cancellationToken);
        Console.WriteLine($"Configuration saved to {configPath ?? $"{ConfigReader.CONFIG_FILE_BASE}.json"}");
        return true;
    }
}
