using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using XrmSync.Actions;
using XrmSync.AssemblyAnalyzer.Extensions;
using XrmSync.Dataverse.Extensions;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Options;
using XrmSync.SyncService.Extensions;

namespace XrmSync.Commands;

internal class PluginSyncCommand : XrmSyncSyncCommandBase
{
    private readonly Option<string> _assemblyFile;

    public PluginSyncCommand() : base("plugins", "Synchronize plugins in a plugin assembly with Dataverse")
    {
        _assemblyFile = new("--assembly", "--assembly-file", "-a", "--af")
        {
            Description = "Path to the plugin assembly (*.dll)",
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
        var (saveConfig, saveConfigTo, configName) = GetSharedOptionValues(parseResult);
        
        // Build service provider
        var configReader = new ConfigReader();
        var resolvedConfigName = configReader.ResolveConfigurationName(configName);

        var serviceProvider = GetPluginSyncServices()
            .AddXrmSyncConfiguration(resolvedConfigName, builder =>
            {
                var baseOptions = builder.Build();
                var basePluginSyncOptions = baseOptions.Plugin?.Sync;

                var pluginSyncOptions = new PluginSyncOptions(
                    string.IsNullOrWhiteSpace(assemblyPath) ? basePluginSyncOptions?.AssemblyPath ?? string.Empty : assemblyPath,
                    string.IsNullOrWhiteSpace(solutionName) ? basePluginSyncOptions?.SolutionName ?? string.Empty : solutionName,
                    logLevel ?? basePluginSyncOptions?.LogLevel ?? Microsoft.Extensions.Logging.LogLevel.Information,
                    dryRun || (basePluginSyncOptions?.DryRun ?? false)
                );

                return new XrmSyncConfiguration(new PluginOptions(pluginSyncOptions, baseOptions.Plugin?.Analysis));
            })
            .AddLogger(
                sp => sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<XrmSyncConfiguration>>().Value.Plugin?.Sync?.LogLevel,
                ciMode
            )
            .BuildServiceProvider();

        return await RunAction(serviceProvider, saveConfigTo, ConfigurationScope.PluginSync, cancellationToken)
            ? E_OK
            : E_ERROR;
    }

    private static IServiceCollection GetPluginSyncServices(IServiceCollection? services = null)
    {
        services ??= new ServiceCollection();

        services.AddSingleton<IAction, PluginSyncAction>();
        services.AddSingleton<ISaveConfigAction, SavePluginSyncConfigAction>();

        services.AddSyncService();
        services.AddAssemblyReader();
        services.AddDataverseConnection();

        return services;
    }
}
