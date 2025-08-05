using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Text.Json;
using XrmSync;
using XrmSync.AssemblyAnalyzer;
using XrmSync.Model;
using DGLoggerFactory = XrmSync.LoggerFactory;

// Define CLI options
Option<string> assemblyFileOption = new(["--assembly", "-a", "--assembly-file", "--af"], "Path to the plugin assembly (*.dll)")
{
    IsRequired = true,
    Arity = ArgumentArity.ExactlyOne
};

var solutionNameOption = new Option<string>(["--solution-name", "--sn", "-n"], "Name of the solution")
{
    IsRequired = true,
    Arity = ArgumentArity.ExactlyOne
};

var dryRunOption = new Option<bool>(["--dry-run", "--dryrun"], "Perform a dry run without making changes")
{
    IsRequired = false
};

var logLevelOption = new Option<LogLevel>(["--log-level", "-l"], () => LogLevel.Information, "Set the minimum log level (Trace, Debug, Information, Warning, Error, Critical)");
var dataverseOption = new Option<string?>(["--dataverse"], "The Dataverse URL to connect to");

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
    assemblyFileOption
};

analyzeAssemblyCommand.SetHandler((assemblyPath) =>
{
    try
    {
        var pluginDto = AssemblyAnalyzer.GetPluginAssembly(assemblyPath);
        var jsonOutput = JsonSerializer.Serialize(pluginDto);
        Console.WriteLine(jsonOutput);
    }
    catch (AnalysisException ex)
    {
        Console.Error.WriteLine($"Error analyzing assembly: {ex.Message}");
        Environment.Exit(1);
    }
}, assemblyFileOption);

rootCommand.AddCommand(analyzeAssemblyCommand);

rootCommand.SetHandler(async (assemblyPath, solutionName, dryRun, logLevel, dataverseUrl) =>
{
    DGLoggerFactory.MinimumLevel = logLevel;

    var options = new XrmSyncOptions
    {
        AssemblyPath = assemblyPath,
        SolutionName = solutionName,
        DryRun = dryRun,
        LogLevel = logLevel.ToString(),
        DataverseUrl = dataverseUrl
    };

    if (!await PluginSync.RunSync(options))
    {
        Environment.Exit(1);
    }

}, assemblyFileOption, solutionNameOption, dryRunOption, logLevelOption, dataverseOption);

return await rootCommand.InvokeAsync(args);
