using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using XrmSync.Extensions;
using XrmSync.Options;

namespace XrmSync.Commands;

internal class ConfigListCommand : XrmSyncCommandBase
{
    public ConfigListCommand() : base("list", "List all available named configurations")
    {
        AddSharedOptions();

        SetAction(ExecuteAsync);
    }

    private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var sharedOptions = GetSharedOptionValues(parseResult);

        // Build service provider
        var serviceProvider = GetConfigListServices()
            .AddXrmSyncConfiguration(sharedOptions)
            .AddOptions(baseOptions => baseOptions) // No overrides needed for list
            .BuildServiceProvider();

        try
        {
            var output = serviceProvider.GetRequiredService<IConfigValidationOutput>();
            await output.OutputConfigList(cancellationToken);

            return E_OK;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Configuration list error:{Environment.NewLine}{ex.Message}");
            return E_ERROR;
        }
    }

    private static IServiceCollection GetConfigListServices(IServiceCollection? services = null)
    {
        services ??= new ServiceCollection();

        services.AddSingleton<IConfigValidationOutput, ConfigValidationOutput>();

        return services;
    }
}
