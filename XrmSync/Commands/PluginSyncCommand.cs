using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using XrmSync.Constants;
using XrmSync.Extensions;
using XrmSync.Options;
using XrmSync.SyncService.Extensions;

namespace XrmSync.Commands;

internal class PluginSyncCommand : XrmSyncSyncCommandBase
{
    private readonly Option<string> _assemblyFile;

    public PluginSyncCommand() : base("plugins", "Synchronize plugins in a plugin assembly with Dataverse")
    {
        _assemblyFile = new(CliOptions.Assembly.Primary, CliOptions.Assembly.Aliases)
        {
            Description = CliOptions.Assembly.Description,
            Arity = ArgumentArity.ExactlyOne
        };

        Add(_assemblyFile);

        AddSharedOptions();
        AddSyncSharedOptions();

        SetAction(ExecuteAsync);
    }

    private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var assemblyPath = parseResult.GetValue(_assemblyFile);
        var (solutionName, dryRun, logLevel, ciMode) = GetSyncSharedOptionValues(parseResult);
        var sharedOptions = GetSharedOptionValues(parseResult);
        
        // Build service provider
        var serviceProvider = GetPluginSyncServices()
            .AddXrmSyncConfiguration(sharedOptions)
            .AddOptions(
                baseOptions => baseOptions with
                {
                    Logger = baseOptions.Logger with
                    {
                        LogLevel = logLevel ?? baseOptions.Logger.LogLevel,
                        CiMode = ciMode ?? baseOptions.Logger.CiMode
                    },
                    Execution = baseOptions.Execution with
                    {
                        DryRun = dryRun ?? baseOptions.Execution.DryRun
                    },
                    Plugin = baseOptions.Plugin with
                    {
                        Sync = new(
                            string.IsNullOrWhiteSpace(assemblyPath) ? baseOptions.Plugin.Sync.AssemblyPath : assemblyPath,
                            string.IsNullOrWhiteSpace(solutionName) ? baseOptions.Plugin.Sync.SolutionName : solutionName
                        )
                    }
                })
            .AddCommandOptions(c => c.Plugin.Sync)
            .AddLogger()
            .BuildServiceProvider();

        return await RunAction(serviceProvider, ConfigurationScope.PluginSync, CommandAction, cancellationToken)
            ? E_OK
            : E_ERROR;
    }

    private static IServiceCollection GetPluginSyncServices(IServiceCollection? services = null)
    {
        services ??= new ServiceCollection();

        services.AddPluginSyncService();

        return services;
    }
}
