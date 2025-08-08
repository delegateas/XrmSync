using Microsoft.Extensions.Logging;
using System.CommandLine;

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

        var prettyPrintOption = new Option<bool>(["--pretty-print", "-p"], "Pretty print the JSON output")
        {
            IsRequired = false
        };

        var rootCommand = new RootCommand("XrmSync - Synchronize your Dataverse plugins")
        {
            assemblyFileOption,
            solutionNameOption,
            dryRunOption,
            logLevelOption
        };

        var analyzeAssemblyCommand = new Command("analyze", "Analyze a plugin assembly and output info as JSON")
        {
            assemblyFileOption,
            prettyPrintOption
        };
        rootCommand.AddCommand(analyzeAssemblyCommand);

        var handlers = new CommandHandlers();
        analyzeAssemblyCommand.SetHandler((string assemblyPath, bool prettyPrint) => 
        {
            var result = handlers.HandleAnalyze(assemblyPath, prettyPrint);
            Environment.ExitCode = result;
        }, assemblyFileOption, prettyPrintOption);

        rootCommand.SetHandler(async (string assemblyPath, string solutionName, bool dryRun, LogLevel? logLevel) => 
        {
            var result = await handlers.HandleSync(assemblyPath, solutionName, dryRun, logLevel);
            Environment.ExitCode = result;
        }, assemblyFileOption, solutionNameOption, dryRunOption, logLevelOption);

        return rootCommand;
    }
}
