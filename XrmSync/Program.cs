using DG.XrmSync;
using DG.XrmSync.Dataverse.Extensions;
using DG.XrmSync.Model;
using DG.XrmSync.SyncService;
using DG.XrmSync.SyncService.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.Text.Json;
using DG.XrmSync.AssemblyAnalyzer;
using DGLoggerFactory = DG.XrmSync.LoggerFactory;
using DG.XrmSync.AssemblyAnalyzer.Extensions;


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
    var pluginDto = AssemblyAnalyzer.GetPluginAssembly(assemblyPath);
    var jsonOutput = JsonSerializer.Serialize(pluginDto);
    Console.WriteLine(jsonOutput);
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

    await PluginSync.RunSync(host.Services);
}, assemblyFileOption, solutionNameOption, dryRunOption, logLevelOption, dataverseOption);

return await rootCommand.InvokeAsync(args);
