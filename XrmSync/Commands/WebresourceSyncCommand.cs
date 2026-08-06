using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Model.Webresource;
using XrmSync.Options;
using XrmSync.SyncService.Extensions;
using XrmSync.Watch;
using MSOptions = Microsoft.Extensions.Options.Options;

namespace XrmSync.Commands
{
	internal class WebresourceSyncCommand : XrmSyncCommandBase
	{
		public WebresourceSyncCommand() : base("webresources", "Synchronize webresources from a local folder with Dataverse")
		{
			Add(CommandOptions.Folder);
			Add(CommandOptions.FileExtensions);
			Add(CommandOptions.Watch);

			AddSharedOptions();
			AddSyncOptions();

			SetAction(ExecuteAsync);
		}

		public override async Task<int?> ExecuteFromProfile(SyncItem syncItem, ExecutionContext ctx, CancellationToken ct)
		{
			if (syncItem is not WebresourceSyncItem webresource) return null;
			return await RunCore(webresource.FolderPath, ctx.SolutionName ?? string.Empty, webresource.FileExtensions, ctx.WatchSession, ctx.DryRun, ctx.CiMode, ctx.LogLevel, ctx.ProfileName, ct);
		}

		private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
		{
			var folderPath = parseResult.GetValue(CommandOptions.Folder);
			var extensionsValue = parseResult.GetValue(CommandOptions.FileExtensions);
			var solutionName = parseResult.GetValue(CommandOptions.Solution);
			var watch = parseResult.GetValue(CommandOptions.Watch);
			var (dryRun, ciMode, logLevel, profileName) = ReadExecutionOverrides(parseResult);

			var (profile, exitCode) = ResolveCommandProfile(profileName,
				!string.IsNullOrWhiteSpace(folderPath) && !string.IsNullOrWhiteSpace(solutionName),
				"Specify --folder and --solution, or add a profile to appsettings.json.");
			if (exitCode.HasValue) return exitCode.Value;

			// Sync item is optional — its solution name falls back to the profile-level shared value
			var item = profile?.Sync.OfType<WebresourceSyncItem>().FirstOrDefault();

			var finalFolderPath = folderPath.GetValueOrDefault(item?.FolderPath ?? string.Empty);
			var finalSolutionName = solutionName.GetValueOrDefault(profile?.ResolveSolutionName(item) ?? string.Empty);
			var finalExtensions = extensionsValue is { Length: > 0 } ? [.. extensionsValue] : item?.FileExtensions;

			var watchSettings = ResolveWatchSettings(watch, item?.Watch ?? false, ciMode);

			// The whole watch session publishes, including the initial pass — otherwise files uploaded at
			// startup would stay unpublished until they happen to be touched again
			var initialResult = await RunCore(finalFolderPath, finalSolutionName, finalExtensions, watchSettings.Enabled, dryRun, ciMode, logLevel, profileName, cancellationToken);

			if (!watchSettings.Enabled)
				return initialResult;

			var target = WatchTargetResolver.ForFolder(finalFolderPath, finalExtensions, item ?? WebresourceSyncItem.Empty);
			if (target == null)
				return initialResult;

			await CreateWatchLoop(watchSettings, dryRun, ciMode, logLevel, profileName)
				.RunAsync([target], (_, ct) => RunCore(finalFolderPath, finalSolutionName, finalExtensions, true, dryRun, ciMode, logLevel, profileName, ct), cancellationToken);

			return initialResult;
		}

		private async Task<int> RunCore(
			string folderPath,
			string solutionName,
			List<string>? fileExtensionsList,
			bool publish,
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
				.AddXrmSyncConfiguration(new ExecutionContext(null, null, null, null, profileName))
				.AddOptions(dryRun, ciMode, logLevel)
				.AddSingleton(MSOptions.Create(new WebresourceSyncCommandOptions(folderPath, solutionName, fileExtensionsList, publish)))
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
