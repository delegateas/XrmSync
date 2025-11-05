using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using XrmSync.Extensions;
using XrmSync.Options;

namespace XrmSync.Commands;

internal class ConfigValidateCommand : XrmSyncCommandBase
{
    public ConfigValidateCommand() : base("validate", "Validate configuration from appsettings.json")
    {
        AddSharedOptions();

        SetAction(ExecuteAsync);
    }

    private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
    {
        var sharedOptions = GetSharedOptionValues(parseResult);

        // Build service provider - no overrides, just validate what's in the config
        var serviceProvider = GetConfigValidateServices()
            .AddXrmSyncConfiguration(sharedOptions)
            .AddOptions(baseOptions => baseOptions) // No overrides
            .BuildServiceProvider();

        try
        {
            var output = serviceProvider.GetRequiredService<IConfigValidationOutput>();
            await output.OutputValidationResult(cancellationToken);

            return E_OK;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Configuration validation error:{Environment.NewLine}{ex.Message}");
            return E_ERROR;
        }
    }

    private static IServiceCollection GetConfigValidateServices(IServiceCollection? services = null)
    {
        services ??= new ServiceCollection();

        services.AddSingleton<IConfigValidationOutput, ConfigValidationOutput>();

        return services;
    }
}
