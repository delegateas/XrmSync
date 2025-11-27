using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using XrmSync.Constants;
using XrmSync.Model.Exceptions;
using XrmSync.SyncService;

namespace XrmSync.Commands
{
	internal abstract class XrmSyncSyncCommandBase(string name, string description) : XrmSyncCommandBase(name, description)
	{
		protected Option<string> SolutionName { get; private set; } = null!;
		protected Option<bool?> DryRun { get; private set; } = null!;
		protected Option<LogLevel?> LogLevel { get; private set; } = null!;
		protected Option<bool?> CiMode { get; private set; } = null!;

		protected virtual void AddSyncSharedOptions()
		{
			SolutionName = new(CliOptions.Solution.Primary, CliOptions.Solution.Aliases)
			{
				Description = CliOptions.Solution.Description,
				Arity = ArgumentArity.ExactlyOne
			};

			DryRun = new(CliOptions.Execution.DryRun.Primary, CliOptions.Execution.DryRun.Aliases)
			{
				Description = CliOptions.Execution.DryRun.Description,
				Required = false
			};

			LogLevel = new(CliOptions.Logging.LogLevel.Primary, CliOptions.Logging.LogLevel.Aliases)
			{
				Description = CliOptions.Logging.LogLevel.Description
			};

			CiMode = new(CliOptions.Logging.CiMode.Primary, CliOptions.Logging.CiMode.Aliases)
			{
				Description = CliOptions.Logging.CiMode.Description,
				Required = false
			};

			Add(SolutionName);
			Add(DryRun);
			Add(LogLevel);
			Add(CiMode);
		}

		protected (string? SolutionName, bool? DryRun, LogLevel? LogLevel, bool? CIMode) GetSyncSharedOptionValues(ParseResult parseResult)
		{
			var solutionName = parseResult.GetValue(SolutionName);
			var dryRun = parseResult.GetValue(DryRun);
			var logLevel = parseResult.GetValue(LogLevel);
			var ciMode = parseResult.GetValue(CiMode);

			return (solutionName, dryRun, logLevel, ciMode);
		}

		protected static async Task<bool> CommandAction(IServiceProvider serviceProvider, CancellationToken cancellationToken)
		{
			var logger = serviceProvider.GetRequiredService<ILogger<XrmSyncSyncCommandBase>>();
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
}
