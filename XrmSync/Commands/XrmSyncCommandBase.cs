using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using XrmSync.Constants;
using XrmSync.Model.Exceptions;
using XrmSync.Options;

namespace XrmSync.Commands;

internal record SharedOptions(bool SaveConfig, string? SaveConfigTo, string ConfigName)
{
    public static SharedOptions Empty => new (false, null, XrmSyncConfigurationBuilder.DEFAULT_CONFIG_NAME);
}

/// <summary>
/// Abstract base class for XrmSync commands with common functionality
/// </summary>
internal abstract class XrmSyncCommandBase(string name, string description) : Command(name, description), IXrmSyncCommand
{
    protected const int E_OK = 0;
    protected const int E_ERROR = 1;

    // Shared options available to all commands
    protected Option<bool> SaveConfigOption { get; private set; } = null!;
    protected Option<string?> SaveConfigToOption { get; private set; } = null!;
    protected Option<string?> ConfigNameOption { get; private set; } = null!;

    public Command GetCommand() => this;

    /// <summary>
    /// Adds shared options to the command (save-config, save-config-to, config-name)
    /// </summary>
    protected void AddSharedOptions()
    {
        SaveConfigOption = new(CliOptions.Config.SaveConfig.Primary, CliOptions.Config.SaveConfig.Aliases)
        {
            Description = CliOptions.Config.SaveConfig.Description,
            Required = false
        };

        SaveConfigToOption = new(CliOptions.Config.SaveConfigTo.Primary)
        {
            Description = CliOptions.Config.SaveConfigTo.Description,
            Required = false
        };

        ConfigNameOption = new(CliOptions.Config.LoadConfig.Primary, CliOptions.Config.LoadConfig.Aliases)
        {
            Description = CliOptions.Config.LoadConfig.Description,
            Required = false
        };

        Add(SaveConfigOption);
        Add(SaveConfigToOption);
        Add(ConfigNameOption);
    }

    /// <summary>
    /// Gets the shared option values from a parse result
    /// </summary>
    protected SharedOptions GetSharedOptionValues(ParseResult parseResult)
    {
        var saveConfig = parseResult.GetValue(SaveConfigOption);
        var saveConfigTo = saveConfig ? parseResult.GetValue(SaveConfigToOption) ?? ConfigReader.CONFIG_FILE_BASE + ".json" : null;
        var configName = parseResult.GetValue(ConfigNameOption) ?? XrmSyncConfigurationBuilder.DEFAULT_CONFIG_NAME;

        return new (saveConfig, saveConfigTo, configName);
    }

    /// <summary>
    /// Validates configuration and runs the appropriate action (save config or execute)
    /// </summary>
    protected static async Task<bool> RunAction(
        IServiceProvider serviceProvider,
        ConfigurationScope configurationScope,
        Func<IServiceProvider, CancellationToken, Task<bool>> action,
        CancellationToken cancellationToken)
    {
        // Validate options before taking further action
        try
        {
            var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();
            validator.Validate(configurationScope);
        }
        catch (OptionsValidationException ex)
        {
            Console.Error.WriteLine($"Configuration validation failed:{Environment.NewLine}{ex.Message}");
            return false;
        }

        var sharedOptions = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SharedOptions>>();

        var (saveConfig, saveConfigTo, configName) = sharedOptions.Value;
        if (saveConfig)
        {
            var configWriter = serviceProvider.GetRequiredService<IConfigWriter>();

            var configPath = string.IsNullOrWhiteSpace(saveConfigTo) ? null : saveConfigTo;
            
            await configWriter.SaveConfig(configPath, configName, cancellationToken);
            Console.WriteLine($"Configuration saved to {saveConfigTo}");
            return true;
        }
        else
        {
            return await action(serviceProvider, cancellationToken);
        }
    }
}
