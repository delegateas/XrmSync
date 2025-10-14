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

    public Command GetCommand() => this;

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
