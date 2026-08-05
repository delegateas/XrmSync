using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Extensions;
using XrmSync.Options;
using XrmSync.SyncService;
using XrmSync.Watch;

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
	/// Default implementation: this command does not handle profile sync items.
	/// Override in sync sub-commands to handle a specific SyncItem subtype.
	/// </summary>
	public virtual Task<int?> ExecuteFromProfile(SyncItem syncItem, ExecutionContext ctx, CancellationToken ct)
		=> Task.FromResult<int?>(null);

	/// <summary>
	/// Adds the --profile option to the command
	/// </summary>
	protected void AddSharedOptions() => Add(CommandOptions.Profile);

	/// <summary>
	/// Adds sync-specific shared options: --solution, --dry-run, --log-level, --ci-mode
	/// </summary>
	protected void AddSyncOptions()
	{
		Add(CommandOptions.Solution);
		Add(CommandOptions.DryRun);
		Add(CommandOptions.LogLevel);
		Add(CommandOptions.CiMode);
	}

	/// <summary>
	/// The execution-level CLI overrides shared by every sync sub-command (analysis aside, which only reads
	/// the profile name). Deconstructs in declaration order, e.g.
	/// <c>var (dryRun, ciMode, logLevel, profileName) = ReadExecutionOverrides(parseResult);</c>
	/// </summary>
	protected readonly record struct ExecutionOverrides(bool? DryRun, bool? CiMode, LogLevel? LogLevel, string? ProfileName);

	/// <summary>
	/// Reads the shared --dry-run / --ci-mode / --log-level / --profile values from the parse result.
	/// </summary>
	protected static ExecutionOverrides ReadExecutionOverrides(ParseResult parseResult) => new(
		parseResult.GetValue(CommandOptions.DryRun),
		parseResult.GetValue(CommandOptions.CiMode),
		parseResult.GetValue(CommandOptions.LogLevel),
		parseResult.GetValue(CommandOptions.Profile));

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
	/// Resolves the profile a sub-command should merge its CLI values against.
	/// <para>
	/// Returns a <c>null</c> profile and no exit code for "standalone" execution — when no profile was
	/// requested and the caller already has every required value from the CLI; callers then merge against
	/// empty fallbacks (CLI values win). Otherwise the requested/default profile is loaded, and an exit code
	/// is returned when it cannot be resolved (unknown profile name, or none configured).
	/// </para>
	/// The merge itself stays in each command via <c>cliValue.GetValueOrDefault(profile?.…)</c>, so the same
	/// expression covers both standalone (profile is null) and profile execution.
	/// </summary>
	/// <param name="standaloneInputsComplete">Whether the CLI already supplies every value required to run without a profile.</param>
	/// <param name="missingProfileHint">Command-specific hint appended to the "No profiles configured" message.</param>
	protected static (ProfileConfiguration? Profile, int? ExitCode) ResolveCommandProfile(
		string? profileName,
		bool standaloneInputsComplete,
		string missingProfileHint)
	{
		if (profileName == null && standaloneInputsComplete)
		{
			// Standalone mode: all required values supplied via CLI, no profile needed
			return (null, null);
		}

		ProfileConfiguration? profile;
		try
		{
			profile = LoadProfileAndConfig(profileName).Profile;
		}
		catch (XrmSyncException ex)
		{
			Console.Error.WriteLine(ex.Message);
			return (null, E_ERROR);
		}

		if (profile == null)
		{
			Console.Error.WriteLine($"No profiles configured. {missingProfileHint}");
			return (null, E_ERROR);
		}

		return (profile, null);
	}

	/// <summary>
	/// Loads only the global configuration settings, never throwing — used on the standalone command
	/// paths where no profile is resolved but global settings (e.g. watch debounce) are still needed.
	/// </summary>
	protected static XrmSyncConfiguration LoadGlobalConfig()
	{
		try
		{
			return new XrmSyncConfigurationBuilder(new ConfigReader().GetConfiguration()).Build();
		}
		catch (Exception)
		{
			return XrmSyncConfiguration.Empty;
		}
	}

	/// <summary>
	/// Resolves watch behaviour for a sub-command: the --watch flag wins over the sync item's Watch
	/// setting, and CI mode disables watching altogether. Warns on stderr when it is suppressed.
	/// </summary>
	protected static WatchSettings ResolveWatchSettings(bool? cliWatch, bool itemWatch, bool? ciModeOverride)
	{
		var config = LoadGlobalConfig();
		var settings = WatchSettings.Resolve(cliWatch, itemWatch, ciModeOverride ?? config.CiMode, config);

		if (settings.Suppressed)
		{
			Console.Error.WriteLine("Watch mode is not supported in CI mode — running once and exiting.");
		}

		return settings;
	}

	/// <summary>
	/// Builds a watch loop that logs through the standard XrmSync logger configuration.
	/// </summary>
	protected static IWatchLoop CreateWatchLoop(WatchSettings settings, bool? dryRun, bool? ciMode, LogLevel? logLevel, string? profileName)
	{
		var logger = new ServiceCollection()
			.AddXrmSyncConfiguration(new ExecutionContext(null, null, null, null, profileName))
			.AddOptions(dryRun, ciMode, logLevel)
			.AddLogger()
			.BuildServiceProvider()
			.GetRequiredService<ILogger<WatchLoop>>();

		return new WatchLoop(new WatchFileSystem(), logger, settings);
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
