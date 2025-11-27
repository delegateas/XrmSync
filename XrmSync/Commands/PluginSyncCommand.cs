using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.CommandLine;
using XrmSync.Constants;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Options;
using XrmSync.SyncService.Extensions;

namespace XrmSync.Commands;

internal class PluginSyncCommand : XrmSyncSyncCommandBase
{
	private readonly Option<string> assemblyFile;

	public PluginSyncCommand() : base("plugins", "Synchronize plugins in a plugin assembly with Dataverse")
	{
		assemblyFile = new(CliOptions.Assembly.Primary, CliOptions.Assembly.Aliases)
		{
			Description = CliOptions.Assembly.Description,
			Arity = ArgumentArity.ZeroOrOne
		};

		Add(assemblyFile);

		AddSharedOptions();
		AddSyncSharedOptions();

		SetAction(ExecuteAsync);
	}

	private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
	{
		var assemblyPath = parseResult.GetValue(assemblyFile);
		var (solutionName, dryRun, logLevel, ciMode) = GetSyncSharedOptionValues(parseResult);
		var sharedOptions = GetSharedOptionValues(parseResult);

		// Build service provider
		var serviceProvider = GetPluginSyncServices()
			.AddXrmSyncConfiguration(sharedOptions)
			.AddOptions(
				baseOptions => baseOptions with
				{
					LogLevel = logLevel ?? baseOptions.LogLevel,
					CiMode = ciMode ?? baseOptions.CiMode,
					DryRun = dryRun ?? baseOptions.DryRun
				})
			.AddSingleton(sp =>
			{
				var config = sp.GetRequiredService<IOptions<XrmSyncConfiguration>>().Value;

				// Determine assembly path and solution name
				string finalAssemblyPath;
				string finalSolutionName;

				// If CLI options provided, use them (standalone mode)
				if (!string.IsNullOrWhiteSpace(assemblyPath) && !string.IsNullOrWhiteSpace(solutionName))
				{
					finalAssemblyPath = assemblyPath;
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
							"Either specify --assembly and --solution, or use --profile with a valid profile name.");
					}

					var pluginSyncItem = profile.Sync.OfType<PluginSyncItem>().FirstOrDefault();
					if (pluginSyncItem == null)
					{
						throw new InvalidOperationException(
							$"Profile '{profile.Name}' does not contain a Plugin sync item. " +
							"Either specify --assembly and --solution, or use a profile with a Plugin sync item.");
					}

					finalAssemblyPath = !string.IsNullOrWhiteSpace(assemblyPath)
						? assemblyPath
						: pluginSyncItem.AssemblyPath;
					finalSolutionName = !string.IsNullOrWhiteSpace(solutionName)
						? solutionName
						: profile.SolutionName;
				}

				return Microsoft.Extensions.Options.Options.Create(new PluginSyncCommandOptions(finalAssemblyPath, finalSolutionName));
			})
			.AddSingleton(sp =>
			{
				var config = sp.GetRequiredService<IOptions<XrmSyncConfiguration>>().Value;
				return Microsoft.Extensions.Options.Options.Create(new ExecutionModeOptions(config.DryRun));
			})
			.AddLogger()
			.BuildServiceProvider();

		return await RunAction(serviceProvider, ConfigurationScope.PluginSync, CommandAction, cancellationToken)
			? E_OK
			: E_ERROR;
	}

	private static IServiceCollection GetPluginSyncServices(IServiceCollection? services = null)
	{
		services ??= new ServiceCollection();

		services.AddPluginSyncService();

		return services;
	}
}
