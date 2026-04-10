using Microsoft.Extensions.DependencyInjection;
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
	internal class WebresourceSyncCommand : XrmSyncSyncCommandBase
	{
		private readonly Option<string> webresourceRoot;
		private readonly Option<string[]> fileExtensions;

		// Root-level override options (advertised to XrmSyncRootCommand via GetProfileOverrides)
		private readonly Option<string?> rootFolder = CliOptions.Webresource.CreateOption<string?>();
		private readonly Option<string[]?> rootFileExtensions = CliOptions.FileExtensions.CreateOption<string[]?>();

		public WebresourceSyncCommand() : base("webresources", "Synchronize webresources from a local folder with Dataverse")
		{
			webresourceRoot = new(CliOptions.Webresource.Primary, CliOptions.Webresource.Aliases)
			{
				Description = CliOptions.Webresource.Description,
				Arity = ArgumentArity.ZeroOrOne
			};

			fileExtensions = new(CliOptions.FileExtensions.Primary, CliOptions.FileExtensions.Aliases)
			{
				Description = CliOptions.FileExtensions.Description,
				Arity = ArgumentArity.ZeroOrMore,
				AllowMultipleArgumentsPerToken = true
			};

			Add(webresourceRoot);
			Add(fileExtensions);

			AddSharedOptions();
			AddSyncSharedOptions();

			SetAction(ExecuteAsync);
		}

		/// <summary>
		/// Advertises --folder and --file-extensions as root-level overrides.
		/// The shared solution option is used in the merge callback but owned by the root command.
		/// </summary>
		public override ProfileOverrideProvider? GetProfileOverrides(Option<string?> assembly, Option<string?> solution) => new(
			options: [rootFolder, rootFileExtensions],
			mergeSyncItem: (item, parseResult) =>
			{
				if (item is not WebresourceSyncItem webresource) return null;
				var folderValue = parseResult.GetValue(rootFolder);
				var extensions = parseResult.GetValue(rootFileExtensions);
				return webresource with
				{
					FolderPath = !string.IsNullOrWhiteSpace(folderValue) ? folderValue : webresource.FolderPath,
					FileExtensions = extensions is { Length: > 0 } ? extensions.ToList() : webresource.FileExtensions
				};
			});

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

				finalFolderPath = !string.IsNullOrWhiteSpace(folderPath) ? folderPath : (webresourceSyncItem?.FolderPath ?? string.Empty);
				finalSolutionName = !string.IsNullOrWhiteSpace(solutionName) ? solutionName : profile.SolutionName;
				finalExtensions = extensionsValue is { Length: > 0 } ? extensionsValue.ToList() : webresourceSyncItem?.FileExtensions;
			}

			// Validate resolved values
			var errors = XrmSyncConfigurationValidator.ValidateFolderPath(finalFolderPath)
				.Concat(XrmSyncConfigurationValidator.ValidateSolutionName(finalSolutionName))
				.ToList();
			if (errors.Count > 0)
				return ValidationError("webresources", errors);

			// Build service provider with validated options
			var serviceProvider = GetWebresourceSyncServices()
				.AddXrmSyncConfiguration(sharedOptions)
				.AddOptions(
					options => options with
					{
						LogLevel = logLevel ?? options.LogLevel,
						CiMode = ciMode ?? options.CiMode,
						DryRun = dryRun ?? options.DryRun
					}
				)
				.AddSingleton(MSOptions.Create(new WebresourceSyncCommandOptions(finalFolderPath, finalSolutionName, finalExtensions)))
				.AddSingleton(sp =>
				{
					var config = sp.GetRequiredService<IOptions<XrmSyncConfiguration>>().Value;
					return MSOptions.Create(new ExecutionModeOptions(config.DryRun));
				})
				.AddLogger()
				.BuildServiceProvider();

			return await RunAction(serviceProvider, ConfigurationScope.None, CommandAction, cancellationToken)
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
