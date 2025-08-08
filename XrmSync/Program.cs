using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Text.Json;
using XrmSync;
using XrmSync.Model;

// Define CLI options
Option<FileInfo> runOptionsOption = new(["--run", "-r"], "Read settings from the supplied JSON file")
{
    IsRequired = false,
    Arity = ArgumentArity.ExactlyOne
};

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
    dataverseOption,
    runOptionsOption
};

rootCommand.AddValidator(command =>
{
    var assemblyFile = command.GetValueForOption(assemblyFileOption);
    var solutionName = command.GetValueForOption(solutionNameOption);
    var runOptions = command.GetValueForOption(runOptionsOption);

    if (string.IsNullOrWhiteSpace(assemblyFile) && runOptions is null)
    {
        command.ErrorMessage = "Either --assembly or --run option must be provided.";
        return;
    }

    if (!string.IsNullOrWhiteSpace(assemblyFile) && runOptions is null && string.IsNullOrWhiteSpace(solutionName))
    {
        command.ErrorMessage = "When using --assembly, without --run, the --solution-name option must also be provided.";
    }
});

var analyzeAssemblyCommand = new Command("analyze", "Analyze a plugin assembly and output info as JSON")
{
    assemblyFileOption,
    prettyPrintOption
};

analyzeAssemblyCommand.SetHandler((assemblyPath, prettyPrint) =>
{
    if (!PluginSync.RunAnalysis(assemblyPath, prettyPrint))
    {
        Environment.Exit(1);
    }
}, assemblyFileOption, prettyPrintOption);

rootCommand.AddCommand(analyzeAssemblyCommand);

rootCommand.SetHandler(async (assemblyPath, solutionName, dryRun, logLevel, dataverseUrl, runOptions) =>
{
    var options = (runOptions?.Exists == true)
        ? JsonSerializer.Deserialize<XrmSyncOptions>(new FileStream(runOptions.FullName, FileMode.Open)) ?? new XrmSyncOptions()
        : new XrmSyncOptions();

    options.AssemblyPath = string.IsNullOrWhiteSpace(assemblyPath) ? options.AssemblyPath : assemblyPath;
    options.SolutionName = string.IsNullOrWhiteSpace(solutionName) ? options.SolutionName : solutionName;
    options.DryRun = options.DryRun || dryRun;
    options.DataverseUrl = string.IsNullOrWhiteSpace(dataverseUrl) ? options.DataverseUrl : dataverseUrl;
    options.LogLevel = logLevel is null ? options.LogLevel : logLevel.Value.ToString();

    if (!await PluginSync.RunSync(options))
    {
        Environment.Exit(1);
    }
}, assemblyFileOption, solutionNameOption, dryRunOption, logLevelOption, dataverseOption, runOptionsOption);

return await rootCommand.InvokeAsync(args);
