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

namespace XrmSync.Commands;

internal class PluginSyncCommand : XrmSyncCommandBase
{
	private readonly Option<string> assemblyFile;

	public PluginSyncCommand() : base("plugins", "Synchronize plugins in a plugin assembly with Dataverse")
	{
		assemblyFile = CliOptions.Assembly.CreateOption<string>();

		Add(assemblyFile);

		AddSharedOptions();
		AddSyncOptions();

		SetAction(ExecuteAsync);
	}

	public override async Task<int?> ExecuteFromProfile(SyncItem syncItem, ProfileExecutionContext ctx, CancellationToken ct)
	{
		if (syncItem is not PluginSyncItem plugin) return null;
		return await RunCore(plugin.AssemblyPath, ctx.SolutionName, ctx.DryRun, ctx.CiMode, ctx.LogLevel, ctx.ProfileName, ct);
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

			if (profile == null)
			{
				Console.Error.WriteLine("No profiles configured. Specify --assembly and --solution, or add a profile to appsettings.json.");
				return E_ERROR;
			}

			// Sync item is optional — if absent, CLI must supply all plugin-specific values
			var pluginSyncItem = profile.Sync.OfType<PluginSyncItem>().FirstOrDefault();

			finalAssemblyPath = assemblyPath.GetValueOrDefault(pluginSyncItem?.AssemblyPath ?? string.Empty);
			finalSolutionName = solutionName.GetValueOrDefault(profile.SolutionName);
		}

		return await RunCore(finalAssemblyPath, finalSolutionName, dryRun, ciMode, logLevel, sharedOptions.ProfileName, cancellationToken);
	}

	private async Task<int> RunCore(
		string assemblyPath,
		string solutionName,
		bool? dryRun,
		bool? ciMode,
		LogLevel? logLevel,
		string? profileName,
		CancellationToken ct)
	{
		var errors = XrmSyncConfigurationValidator.ValidateAssemblyPath(assemblyPath)
			.Concat(XrmSyncConfigurationValidator.ValidateSolutionName(solutionName))
			.ToList();
		if (errors.Count > 0)
			return ValidationError("plugins", errors);

		var serviceProvider = GetPluginSyncServices()
			.AddXrmSyncConfiguration(new SharedOptions(profileName))
			.AddOptions(
				baseOptions => baseOptions with
				{
					LogLevel = logLevel ?? baseOptions.LogLevel,
					CiMode = ciMode ?? baseOptions.CiMode,
					DryRun = dryRun ?? baseOptions.DryRun
				})
			.AddSingleton(MSOptions.Create(new PluginSyncCommandOptions(assemblyPath, solutionName)))
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

	private static IServiceCollection GetPluginSyncServices(IServiceCollection? services = null)
	{
		services ??= new ServiceCollection();
		services.AddPluginSyncService();
		return services;
	}
}
