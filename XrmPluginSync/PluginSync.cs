using DG.XrmPluginSync.SyncService.Models.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.CommandLine;
using System.Reflection;

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

        var rootCommand = new RootCommand("XrmPluginSync - Synchronize your Dataverse plugins")
        {
            assemblyFileOption,
            solutionNameOption,
            dryRunOption
        };

        rootCommand.SetHandler((solutionFile, solutionName, dryRun) =>
        {
            var req = new SyncRequest
            {
                AssemblyPath = solutionFile.FullName,
                SolutionName = solutionName,
                DryRun = dryRun
            };
            var program = ActivatorUtilities.CreateInstance<PluginSync>(host.Services);
            program.Run(req);
        }, assemblyFileOption, solutionNameOption, dryRunOption);

        return await rootCommand.InvokeAsync(args);
    }
}
