using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.CommandLine;
using XrmSync.Constants;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Options;
using XrmSync.SyncService.Extensions;

namespace XrmSync.Commands
{
	internal class WebresourceSyncCommand : XrmSyncSyncCommandBase
	{
		private readonly Option<string> _webresourceRoot;

		public WebresourceSyncCommand() : base("webresources", "Synchronize webresources from a local folder with Dataverse")
		{
			_webresourceRoot = new(CliOptions.Webresource.Primary, CliOptions.Webresource.Aliases)
			{
				Description = CliOptions.Webresource.Description,
				Arity = ArgumentArity.ZeroOrOne
			};

			Add(_webresourceRoot);

			AddSharedOptions();
			AddSyncSharedOptions();

			SetAction(ExecuteAsync);
		}

		private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
		{
			var folderPath = parseResult.GetValue(_webresourceRoot);
			var (solutionName, dryRun, logLevel, ciMode) = GetSyncSharedOptionValues(parseResult);
			var sharedOptions = GetSharedOptionValues(parseResult);

			// Build service provider
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
				.AddSingleton(sp =>
				{
					var config = sp.GetRequiredService<IOptions<XrmSyncConfiguration>>().Value;

					// Determine folder path and solution name
					string finalFolderPath;
					string finalSolutionName;

					// If CLI options provided, use them (standalone mode)
					if (!string.IsNullOrWhiteSpace(folderPath) && !string.IsNullOrWhiteSpace(solutionName))
					{
						finalFolderPath = folderPath;
						finalSolutionName = solutionName;
					}
					// Otherwise try to get from profile
					else
					{
						var profile = config.Profiles.FirstOrDefault(p =>
							p.Name.Equals(sharedOptions.ProfileName, StringComparison.OrdinalIgnoreCase));

						if (profile == null)
						{
							throw new InvalidOperationException(
								$"Profile '{sharedOptions.ProfileName}' not found. " +
								"Either specify --folder and --solution, or use --profile with a valid profile name.");
						}

						var webresourceSyncItem = profile.Sync.OfType<WebresourceSyncItem>().FirstOrDefault();
						if (webresourceSyncItem == null)
						{
							throw new InvalidOperationException(
								$"Profile '{profile.Name}' does not contain a Webresource sync item. " +
								"Either specify --folder and --solution, or use a profile with a Webresource sync item.");
						}

						finalFolderPath = !string.IsNullOrWhiteSpace(folderPath)
							? folderPath
							: webresourceSyncItem.FolderPath;
						finalSolutionName = !string.IsNullOrWhiteSpace(solutionName)
							? solutionName
							: profile.SolutionName;
					}

					return Microsoft.Extensions.Options.Options.Create(new WebresourceSyncCommandOptions(finalFolderPath, finalSolutionName));
				})
				.AddSingleton(sp =>
				{
					var config = sp.GetRequiredService<IOptions<XrmSyncConfiguration>>().Value;
					return Microsoft.Extensions.Options.Options.Create(new ExecutionModeOptions(config.DryRun));
				})
				.AddLogger()
				.BuildServiceProvider();

			return await RunAction(serviceProvider, ConfigurationScope.WebresourceSync, CommandAction, cancellationToken)
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
