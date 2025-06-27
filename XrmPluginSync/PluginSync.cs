using DG.XrmPluginSync.SyncService.Models.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace DG.XrmPluginSync;

internal class PluginSync(SyncService.SyncService syncService)
{
    public void Run(SyncRequest req)
    {
        syncService.SyncPlugins(req).Wait();
    }

    public static async Task<int> RunCliAsync(string[] args, IHost host)
    {
        Option<FileInfo> assemblyFileOption = new(["--assembly", "-a", "--assembly-file", "--af"], "Path to the plugin assembly (*.dll)")
        {
            IsRequired = true,
            Arity = ArgumentArity.ExactlyOne
        };

        var solutionNameOption = new Option<string>(["--solution-name", "--sn", "-n"], "Name of the solution")
        {
            IsRequired = true,
            Arity = ArgumentArity.ExactlyOne
        };

        var dryRunOption = new Option<bool>(["--dry-run"], "Perform a dry run without making changes")
        {
            IsRequired = false
        };

        var logLevelOption = new Option<LogLevel>(["--log-level", "-l"], () => LogLevel.Information, "Set the minimum log level (Trace, Debug, Information, Warning, Error, Critical)");

        var rootCommand = new RootCommand("XrmPluginSync - Synchronize your Dataverse plugins")
        {
            assemblyFileOption,
            solutionNameOption,
            dryRunOption,
            logLevelOption
        };

        rootCommand.SetHandler((solutionFile, solutionName, dryRun, logLevel) =>
        {
            LoggerFactory.MinimumLevel = logLevel;

            var req = ActivatorUtilities.CreateInstance<SyncRequest>(host.Services);
            req.AssemblyPath = solutionFile.FullName;
            req.SolutionName = solutionName;
            req.DryRun = dryRun;

            var program = ActivatorUtilities.CreateInstance<PluginSync>(host.Services);
            program.Run(req);
        }, assemblyFileOption, solutionNameOption, dryRunOption, logLevelOption);

        return await rootCommand.InvokeAsync(args);
    }
}
