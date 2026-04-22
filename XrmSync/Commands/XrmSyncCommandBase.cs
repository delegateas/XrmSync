using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XrmSync.Constants;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Options;
using XrmSync.SyncService;

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

	// Sync-specific shared options (populated by AddSyncOptions)
	protected Option<string> SolutionName { get; private set; } = null!;
	protected Option<bool?> DryRun { get; private set; } = null!;
	protected Option<LogLevel?> LogLevel { get; private set; } = null!;
	protected Option<bool?> CiMode { get; private set; } = null!;

	public Command GetCommand() => this;

	/// <summary>
	/// Default implementation: this command does not handle profile sync items.
	/// Override in sync sub-commands to handle a specific SyncItem subtype.
	/// </summary>
	public virtual Task<int?> ExecuteFromProfile(SyncItem syncItem, ProfileExecutionContext ctx, CancellationToken ct)
		=> Task.FromResult<int?>(null);

	/// <summary>
	/// Adds the profile option to the command
	/// </summary>
	protected void AddSharedOptions()
	{
		ProfileNameOption = CliOptions.Config.Profile.CreateOption<string?>();
		Add(ProfileNameOption);
	}

	/// <summary>
	/// Adds sync-specific shared options: --solution, --dry-run, --log-level, --ci-mode
	/// </summary>
	protected void AddSyncOptions()
	{
		SolutionName = CliOptions.Solution.CreateOption<string>();
		DryRun = CliOptions.Execution.DryRun.CreateOption<bool?>();
		LogLevel = CliOptions.Logging.LogLevel.CreateOption<LogLevel?>();
		CiMode = CliOptions.Logging.CiMode.CreateOption<bool?>();

		Add(SolutionName);
		Add(DryRun);
		Add(LogLevel);
		Add(CiMode);
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
	/// Gets the sync-specific shared option values from a parse result
	/// </summary>
	protected (string? SolutionName, bool? DryRun, LogLevel? LogLevel, bool? CIMode) GetSyncSharedOptionValues(ParseResult parseResult)
	{
		var solutionName = parseResult.GetValue(SolutionName);
		var dryRun = parseResult.GetValue(DryRun);
		var logLevel = parseResult.GetValue(LogLevel);
		var ciMode = parseResult.GetValue(CiMode);
		return (solutionName, dryRun, logLevel, ciMode);
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
		var dataverseReader = serviceProvider.GetService<IDataverseReader>();
		if (dataverseReader != null)
		{
			var logger = serviceProvider.GetRequiredService<ILogger<XrmSyncCommandBase>>();
			logger.LogInformation("Connected to Dataverse at {dataverseUrl}", dataverseReader.ConnectedHost);
		}

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

	/// <summary>
	/// Standard action for sync commands: runs ISyncService.Sync and handles common exceptions.
	/// </summary>
	protected static async Task<bool> SyncCommandAction(IServiceProvider serviceProvider, CancellationToken cancellationToken)
	{
		var logger = serviceProvider.GetRequiredService<ILogger<XrmSyncCommandBase>>();
		try
		{
			var syncService = serviceProvider.GetRequiredService<ISyncService>();
			await syncService.Sync(cancellationToken);
			return true;
		}
		catch (OptionsValidationException ex)
		{
			logger.LogCritical("Configuration validation failed:{nl}{message}", Environment.NewLine, ex.Message);
			return false;
		}
		catch (XrmSyncException ex)
		{
			logger.LogError("Error during synchronization: {message}", ex.Message);
			return false;
		}
		catch (Exception ex)
		{
			logger.LogCritical(ex, "An unexpected error occurred during synchronization: {message}", ex.Message);
			return false;
		}
	}
}
