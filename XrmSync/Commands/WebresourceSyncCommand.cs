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

				var webresourceSyncItem = profile?.Sync.OfType<WebresourceSyncItem>().FirstOrDefault();
				if (profile == null || webresourceSyncItem == null)
				{
					Console.Error.WriteLine(
						profile == null
							? "No profiles configured. Specify --folder and --solution, or add a profile to appsettings.json."
							: $"Profile '{profile.Name}' does not contain a Webresource sync item. Specify --folder and --solution, or add a Webresource sync item to the profile.");
					return E_ERROR;
				}

				finalFolderPath = !string.IsNullOrWhiteSpace(folderPath) ? folderPath : webresourceSyncItem.FolderPath;
				finalSolutionName = !string.IsNullOrWhiteSpace(solutionName) ? solutionName : profile.SolutionName;
				finalExtensions = extensionsValue is { Length: > 0 } ? extensionsValue.ToList() : webresourceSyncItem.FileExtensions;
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
