using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.CommandLine;
using XrmSync.Constants;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Options;
using XrmSync.SyncService.Extensions;
using MSOptions = Microsoft.Extensions.Options.Options;

namespace XrmSync.Commands
{
	internal class WebresourceSyncCommand : XrmSyncCommandBase
	{
		private readonly Option<string> webresourceRoot;
		private readonly Option<string[]> fileExtensions;

		public WebresourceSyncCommand() : base("webresources", "Synchronize webresources from a local folder with Dataverse")
		{
			webresourceRoot = CliOptions.Webresource.CreateOption<string>();
			fileExtensions = CliOptions.FileExtensions.CreateOption<string[]>();

			Add(webresourceRoot);
			Add(fileExtensions);

			AddSharedOptions();
			AddSyncOptions();

			SetAction(ExecuteAsync);
		}

		public override async Task<int?> ExecuteFromProfile(SyncItem syncItem, ProfileExecutionContext ctx, CancellationToken ct)
		{
			if (syncItem is not WebresourceSyncItem webresource) return null;
			return await RunCore(webresource.FolderPath, ctx.SolutionName, webresource.FileExtensions, ctx.DryRun, ctx.CiMode, ctx.LogLevel, ctx.ProfileName, ct);
		}

		private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
		{
			var folderPath = parseResult.GetValue(webresourceRoot);
			var extensionsValue = parseResult.GetValue(fileExtensions);
			var (solutionName, dryRun, logLevel, ciMode) = GetSyncSharedOptionValues(parseResult);
			var sharedOptions = GetSharedOptionValues(parseResult);

			// Resolve final options eagerly (CLI + profile merge)
			string finalFolderPath;
			string finalSolutionName;
			List<string>? finalExtensions;

			if (sharedOptions.ProfileName == null && !string.IsNullOrWhiteSpace(folderPath) && !string.IsNullOrWhiteSpace(solutionName))
			{
				// Standalone mode: all required values supplied via CLI
				finalFolderPath = folderPath;
				finalSolutionName = solutionName;
				finalExtensions = extensionsValue is { Length: > 0 } ? extensionsValue.ToList() : null;
			}
			else
			{
				// Profile mode: merge profile values with CLI overrides
				ProfileConfiguration? profile;
				try { profile = LoadProfile(sharedOptions.ProfileName); }
				catch (Model.Exceptions.XrmSyncException ex) { Console.Error.WriteLine(ex.Message); return E_ERROR; }

				if (profile == null)
				{
					Console.Error.WriteLine("No profiles configured. Specify --folder and --solution, or add a profile to appsettings.json.");
					return E_ERROR;
				}

				// Sync item is optional — if absent, CLI must supply all webresource-specific values
				var webresourceSyncItem = profile.Sync.OfType<WebresourceSyncItem>().FirstOrDefault();

				finalFolderPath = folderPath.GetValueOrDefault(webresourceSyncItem?.FolderPath ?? string.Empty);
				finalSolutionName = solutionName.GetValueOrDefault(profile.SolutionName);
				finalExtensions = extensionsValue is { Length: > 0 } ? [.. extensionsValue] : webresourceSyncItem?.FileExtensions;
			}

			return await RunCore(finalFolderPath, finalSolutionName, finalExtensions, dryRun, ciMode, logLevel, sharedOptions.ProfileName, cancellationToken);
		}

		private async Task<int> RunCore(
			string folderPath,
			string solutionName,
			List<string>? fileExtensionsList,
			bool? dryRun,
			bool? ciMode,
			LogLevel? logLevel,
			string? profileName,
			CancellationToken ct)
		{
			var errors = XrmSyncConfigurationValidator.ValidateFolderPath(folderPath)
				.Concat(XrmSyncConfigurationValidator.ValidateSolutionName(solutionName))
				.ToList();
			if (errors.Count > 0)
				return ValidationError("webresources", errors);

			var serviceProvider = GetWebresourceSyncServices()
				.AddXrmSyncConfiguration(new SharedOptions(profileName))
				.AddOptions(
					options => options with
					{
						LogLevel = logLevel ?? options.LogLevel,
						CiMode = ciMode ?? options.CiMode,
						DryRun = dryRun ?? options.DryRun
					}
				)
				.AddSingleton(MSOptions.Create(new WebresourceSyncCommandOptions(folderPath, solutionName, fileExtensionsList)))
				.AddSingleton(sp =>
				{
					var config = sp.GetRequiredService<IOptions<XrmSyncConfiguration>>().Value;
					return MSOptions.Create(new ExecutionModeOptions(config.DryRun));
				})
				.AddLogger()
				.BuildServiceProvider();

			return await RunAction(serviceProvider, ConfigurationScope.None, SyncCommandAction, ct)
				? E_OK
				: E_ERROR;
		}

		private static IServiceCollection GetWebresourceSyncServices(IServiceCollection? services = null)
		{
			services ??= new ServiceCollection();
			services.AddWebresourceSyncAction();
			return services;
		}
	}
}
