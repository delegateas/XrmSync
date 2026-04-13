using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using XrmSync.Constants;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Options;
using MSOptions = Microsoft.Extensions.Options.Options;

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
	/// Default implementation: this command advertises no profile overrides.
	/// Override in subclasses to expose sync-item-specific CLI options on the root command.
	/// </summary>
	public virtual ProfileOverrideProvider? GetProfileOverrides(Option<string?> assembly, Option<string?> solution) => null;

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

		return new(profileName);
	}

	/// <summary>
	/// Resolves the profile by name, throwing a consistent error if not found
	/// </summary>
	protected static ProfileConfiguration GetRequiredProfile(IServiceProvider sp, string? profileName, string optionsHint)
	{
		return sp.GetRequiredService<IConfigurationBuilder>().GetProfile(profileName)
			?? throw new InvalidOperationException(
				$"Profile '{profileName}' not found. " +
				$"Either specify {optionsHint}, or use --profile with a valid profile name.");
	}

	/// <summary>
	/// Loads configuration directly and resolves a profile.
	/// Returns null when no profiles are configured.
	/// Throws XrmSyncException when an explicitly requested profile is not found.
	/// </summary>
	protected static ProfileConfiguration? LoadProfile(string? profileName)
	{
		var configuration = new ConfigReader().GetConfiguration();
		return new XrmSyncConfigurationBuilder(configuration).GetProfile(profileName);
	}

	/// <summary>
	/// Loads configuration and resolves a profile, returning both.
	/// Returns null profile when no profiles are configured.
	/// Throws XrmSyncException when an explicitly requested profile is not found.
	/// </summary>
	protected static (ProfileConfiguration? Profile, XrmSyncConfiguration Config) LoadProfileAndConfig(string? profileName)
	{
		var configuration = new ConfigReader().GetConfiguration();
		var builder = new XrmSyncConfigurationBuilder(configuration);
		var config = builder.Build();
		var profile = builder.GetProfile(profileName);
		return (profile, config);
	}

	/// <summary>
	/// Writes validation errors to stderr and returns E_ERROR.
	/// </summary>
	protected static int ValidationError(string prefix, IEnumerable<string> errors)
	{
		Console.Error.WriteLine(new OptionsValidationException(prefix, errors).Message);
		return E_ERROR;
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
