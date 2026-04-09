using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.CommandLine;
using XrmSync.Constants;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Options;
using XrmSync.SyncService.Extensions;
using MSOptions = Microsoft.Extensions.Options.Options;

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

		// Resolve final options eagerly (CLI + profile merge)
		string finalAssemblyPath;
		string finalSolutionName;

		if (sharedOptions.ProfileName == null && !string.IsNullOrWhiteSpace(assemblyPath) && !string.IsNullOrWhiteSpace(solutionName))
		{
			// Standalone mode: all required values supplied via CLI
			finalAssemblyPath = assemblyPath;
			finalSolutionName = solutionName;
		}
		else
		{
			// Profile mode: merge profile values with CLI overrides
			ProfileConfiguration? profile;
			try { profile = LoadProfile(sharedOptions.ProfileName); }
			catch (Model.Exceptions.XrmSyncException ex) { Console.Error.WriteLine(ex.Message); return E_ERROR; }

			var pluginSyncItem = profile?.Sync.OfType<PluginSyncItem>().FirstOrDefault();
			if (profile == null || pluginSyncItem == null)
			{
				Console.Error.WriteLine(
					profile == null
						? "No profiles configured. Specify --assembly and --solution, or add a profile to appsettings.json."
						: $"Profile '{profile.Name}' does not contain a Plugin sync item. Specify --assembly and --solution, or add a Plugin sync item to the profile.");
				return E_ERROR;
			}

			finalAssemblyPath = !string.IsNullOrWhiteSpace(assemblyPath) ? assemblyPath : pluginSyncItem.AssemblyPath;
			finalSolutionName = !string.IsNullOrWhiteSpace(solutionName) ? solutionName : profile.SolutionName;
		}

		// Validate resolved values
		var errors = XrmSyncConfigurationValidator.ValidateAssemblyPath(finalAssemblyPath)
			.Concat(XrmSyncConfigurationValidator.ValidateSolutionName(finalSolutionName))
			.ToList();
		if (errors.Count > 0)
			return ValidationError("plugins", errors);

		// Build service provider with validated options
		var serviceProvider = GetPluginSyncServices()
			.AddXrmSyncConfiguration(sharedOptions)
			.AddOptions(
				baseOptions => baseOptions with
				{
					LogLevel = logLevel ?? baseOptions.LogLevel,
					CiMode = ciMode ?? baseOptions.CiMode,
					DryRun = dryRun ?? baseOptions.DryRun
				})
			.AddSingleton(MSOptions.Create(new PluginSyncCommandOptions(finalAssemblyPath, finalSolutionName)))
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

	private static IServiceCollection GetPluginSyncServices(IServiceCollection? services = null)
	{
		services ??= new ServiceCollection();

		services.AddPluginSyncService();

		return services;
	}
}
