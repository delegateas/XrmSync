using DG.XrmPluginSync;
using DG.XrmPluginSync.Dataverse.Extensions;
using DG.XrmPluginSync.SyncService.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using DGLoggerFactory = DG.XrmPluginSync.LoggerFactory;

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

var dataverseOption = new Option<string?>(["--dataverse"], "The Dataverse URL to connect to")
{
    IsRequired = false
};

var rootCommand = new RootCommand("XrmPluginSync - Synchronize your Dataverse plugins")
{
    assemblyFileOption,
    solutionNameOption,
    dryRunOption,
    logLevelOption,
    dataverseOption
};

rootCommand.SetHandler((assemblyPath, solutionName, dryRun, logLevel, dataverseUrl) =>
{
    DGLoggerFactory.MinimumLevel = logLevel;

    if (!string.IsNullOrEmpty(dataverseUrl))
    {
        Environment.SetEnvironmentVariable("DATAVERSE_URL", dataverseUrl);
    }

    var options = new XrmPluginSyncOptions
    {
        AssemblyPath = assemblyPath,
        SolutionName = solutionName,
        DryRun = dryRun,
        LogLevel = logLevel,
        DataverseUrl = dataverseUrl
    };

    var host = Host.CreateDefaultBuilder()
        .ConfigureServices((_, services) =>
        {
            services.AddSyncService();
            services.AddDataverse();
            services.AddSingleton((_) => DGLoggerFactory.GetLogger<PluginSync>());
            services.AddSingleton<DG.XrmPluginSync.SyncService.Common.Description>();
            services.AddTransient<DG.XrmPluginSync.SyncService.Models.Requests.SyncRequest>();
            services.AddSingleton(options);
        })
        .Build();

    PluginSync.RunCli(host.Services);
}, assemblyFileOption, solutionNameOption, dryRunOption, logLevelOption, dataverseOption);

return await rootCommand.InvokeAsync(args);
