using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using XrmSync.Actions;
using XrmSync.Model.Exceptions;
using XrmSync.Options;

namespace XrmSync.Commands;

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
        SaveConfigOption = new("--save-config", "--sc")
        {
            Description = "Save current CLI options to appsettings.json",
            Required = false
        };

        SaveConfigToOption = new("--save-config-to")
        {
            Description = "If --save-config is set, save to this file instead of appsettings.json",
            Required = false
        };

        ConfigNameOption = new("--config", "--config-name", "-c")
        {
            Description = "Name of the configuration to load from appsettings.json (Default: 'default' or single config if only one exists)",
            Required = false
        };

        Add(SaveConfigOption);
        Add(SaveConfigToOption);
        Add(ConfigNameOption);
    }

    /// <summary>
    /// Gets the shared option values from a parse result
    /// </summary>
    protected (bool saveConfig, string? saveConfigTo, string? configName) GetSharedOptionValues(ParseResult parseResult)
    {
        var saveConfig = parseResult.GetValue(SaveConfigOption);
        var saveConfigTo = saveConfig ? parseResult.GetValue(SaveConfigToOption) ?? ConfigReader.CONFIG_FILE_BASE + ".json" : null;
        var configName = parseResult.GetValue(ConfigNameOption);

        return (saveConfig, saveConfigTo, configName);
    }

    /// <summary>
    /// Validates configuration and runs the appropriate action (save config or execute)
    /// </summary>
    protected static async Task<bool> RunAction(
        IServiceProvider serviceProvider,
        string? saveConfig,
        ConfigurationScope configurationScope,
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

        if (saveConfig is not null)
        {
            var action = serviceProvider.GetRequiredService<ISaveConfigAction>();
            return await action.SaveConfigAsync(saveConfig, cancellationToken);
        }
        else
        {
            var action = serviceProvider.GetRequiredService<IAction>();
            return await action.RunAction(cancellationToken);
        }
    }
}
