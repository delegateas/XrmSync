using Microsoft.Extensions.Logging;
using System.CommandLine;
using XrmSync.Model;

namespace XrmSync;

internal static class CommandLineBuilder
{
    public static RootCommand BuildCommand()
    {
        // Define CLI options
        Option<string> assemblyFileOption = new(["--assembly", "-a", "--assembly-file", "--af"], "Path to the plugin assembly (*.dll)")
        {
            Arity = ArgumentArity.ExactlyOne
        };

        var solutionNameOption = new Option<string>(["--solution-name", "--sn", "-n"], "Name of the solution")
        {
            Arity = ArgumentArity.ExactlyOne
        };

        var dryRunOption = new Option<bool>(["--dry-run", "--dryrun"], "Perform a dry run without making changes")
        {
            IsRequired = false
        };

        var logLevelOption = new Option<LogLevel?>(["--log-level", "-l"], "Set the minimum log level (Trace, Debug, Information, Warning, Error, Critical)");
        var dataverseOption = new Option<string?>(["--dataverse"], "The Dataverse URL to connect to");

        var prettyPrintOption = new Option<bool>(["--pretty-print", "-p"], "Pretty print the JSON output")
        {
            IsRequired = false
        };

        var rootCommand = new RootCommand("XrmSync - Synchronize your Dataverse plugins")
        {
            assemblyFileOption,
            solutionNameOption,
            dryRunOption,
            logLevelOption,
            dataverseOption
        };

        var analyzeAssemblyCommand = new Command("analyze", "Analyze a plugin assembly and output info as JSON")
        {
            assemblyFileOption,
            prettyPrintOption
        };

        analyzeAssemblyCommand.SetHandler(HandleAnalyzeCommand, assemblyFileOption, prettyPrintOption);

        rootCommand.AddCommand(analyzeAssemblyCommand);
        rootCommand.SetHandler(HandleSync, assemblyFileOption, solutionNameOption, dryRunOption, logLevelOption, dataverseOption);

        return rootCommand;
    }

    private static async Task HandleSync(string assemblyPath, string solutionName, bool dryRun, LogLevel? logLevel, string? dataverseUrl)
    {
        var baseConfig = SimpleXrmSyncConfigBuilder.BuildFromConfiguration();

        var config = new XrmSyncOptions(
            string.IsNullOrWhiteSpace(assemblyPath) ? baseConfig.AssemblyPath : assemblyPath,
            string.IsNullOrWhiteSpace(solutionName) ? baseConfig.SolutionName : solutionName,
            logLevel?.ToString() ?? baseConfig.LogLevel,
            dryRun || baseConfig.DryRun,
            string.IsNullOrWhiteSpace(dataverseUrl) ? baseConfig.DataverseUrl : dataverseUrl
        );

        if (!await PluginSync.RunSync(config))
        {
            Environment.Exit(1);
        }
    }

    private static void HandleAnalyzeCommand(string assemblyPath, bool prettyPrint)
    {
        if (!PluginSync.RunAnalysis(assemblyPath, prettyPrint))
        {
            Environment.Exit(1);
        }
    }
}
