using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using XrmSync.Constants;
using XrmSync.Model;
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
    protected Option<string?> ProfileNameOption { get; private set; } = null!;

    public Command GetCommand() => this;

    /// <summary>
    /// Adds shared options to the command (profile)
    /// </summary>
    protected void AddSharedOptions()
    {
        ProfileNameOption = new(CliOptions.Config.Profile.Primary, CliOptions.Config.Profile.Aliases)
        {
            Description = CliOptions.Config.Profile.Description,
            Required = false
        };

        Add(ProfileNameOption);
    }

    /// <summary>
    /// Gets the shared option values from a parse result
    /// </summary>
    protected SharedOptions GetSharedOptionValues(ParseResult parseResult)
    {
        var profileName = parseResult.GetValue(ProfileNameOption);

        return new (profileName);
    }

    /// <summary>
    /// Validates configuration and runs the action
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

        return await action(serviceProvider, cancellationToken);
    }
}
