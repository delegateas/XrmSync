using Microsoft.Extensions.Logging;
using XrmSync.Model;

namespace XrmSync;

internal interface ICommandHandlers
{
    Task<int> HandleSync(string assemblyPath, string solutionName, bool dryRun, LogLevel? logLevel);
    int HandleAnalyze(string assemblyPath, bool prettyPrint);
}

internal class CommandHandlers : ICommandHandlers
{
    public async Task<int> HandleSync(string assemblyPath, string solutionName, bool dryRun, LogLevel? logLevel)
    {
        var baseConfig = SimpleXrmSyncConfigBuilder.BuildFromConfiguration();

        var config = new XrmSyncOptions(
            string.IsNullOrWhiteSpace(assemblyPath) ? baseConfig.AssemblyPath : assemblyPath,
            string.IsNullOrWhiteSpace(solutionName) ? baseConfig.SolutionName : solutionName,
            logLevel?.ToString() ?? baseConfig.LogLevel,
            dryRun || baseConfig.DryRun
        );

        return await PluginSync.RunSync(config) ? 0 : 1;
    }

    public int HandleAnalyze(string assemblyPath, bool prettyPrint)
    {
        return PluginSync.RunAnalysis(assemblyPath, prettyPrint) ? 0 : 1;
    }
}