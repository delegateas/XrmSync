using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Text.Json;
using DGLoggerFactory = XrmSync.LoggerFactory;
using XrmSync;
using XrmSync.Dataverse.Extensions;
using XrmSync.SyncService.Extensions;
using XrmSync.SyncService;
using XrmSync.Model.Exceptions;
using XrmSync.AssemblyAnalyzer;
using XrmSync.AssemblyAnalyzer.Extensions;
using XrmSync.Model;


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

    if (!string.IsNullOrEmpty(dataverseUrl))
    {
        Environment.SetEnvironmentVariable("DATAVERSE_URL", dataverseUrl);
    }

    var options = new XrmSyncOptions
    {
        AssemblyPath = assemblyPath,
        SolutionName = solutionName,
        DryRun = dryRun,
        LogLevel = logLevel.ToString(),
        DataverseUrl = dataverseUrl
    };

    var host = Host.CreateDefaultBuilder()
        .ConfigureServices((_, services) =>
        {
            services.AddSingleton(options);
            services.AddSingleton((_) => DGLoggerFactory.GetLogger<ISyncService>());
            services.AddAssemblyAnalyzer();
            services.AddSyncService();
            services.AddDataverse();
        })
        .Build();

    try
    {
        await PluginSync.RunSync(host.Services);
    } catch (XrmSyncException ex)
    {
        Console.Error.WriteLine($"Error during synchronization: {ex.Message}");
        Environment.Exit(1);
    }
}, assemblyFileOption, solutionNameOption, dryRunOption, logLevelOption, dataverseOption);

return await rootCommand.InvokeAsync(args);
