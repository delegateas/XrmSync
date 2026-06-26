using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Model.Plugin;
using XrmSync.Options;
using XrmSync.SyncService.Extensions;
using MSOptions = Microsoft.Extensions.Options.Options;

namespace XrmSync.Commands;

internal class PluginSyncCommand : XrmSyncCommandBase
{
	public PluginSyncCommand() : base("plugins", "Synchronize plugins in a plugin assembly with Dataverse")
	{
		Add(CommandOptions.Assembly);
		Add(CommandOptions.ClientId);
		Add(CommandOptions.TenantId);
		Add(CommandOptions.AllowEmptyTypes);

		AddSharedOptions();
		AddSyncOptions();

		SetAction(ExecuteAsync);
	}

	public override async Task<int?> ExecuteFromProfile(SyncItem syncItem, ExecutionContext ctx, CancellationToken ct)
	{
		if (syncItem is not PluginSyncItem plugin) return null;
		return await RunCore(plugin.AssemblyPath, ctx.SolutionName ?? string.Empty, plugin.ManagedIdentityClientId, plugin.ManagedIdentityTenantId, plugin.AllowEmptyTypes, ctx.DryRun, ctx.CiMode, ctx.LogLevel, ctx.ProfileName, ct);
	}

	private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
	{
		var assemblyPath = parseResult.GetValue(CommandOptions.Assembly);
		var solutionName = parseResult.GetValue(CommandOptions.Solution);
		var clientId = parseResult.GetValue(CommandOptions.ClientId);
		var tenantId = parseResult.GetValue(CommandOptions.TenantId);
		var allowEmptyTypes = parseResult.GetValue(CommandOptions.AllowEmptyTypes);
		var dryRun = parseResult.GetValue(CommandOptions.DryRun);
		var logLevel = parseResult.GetValue(CommandOptions.LogLevel);
		var ciMode = parseResult.GetValue(CommandOptions.CiMode);
		var profileName = parseResult.GetValue(CommandOptions.Profile);

		// Resolve final options eagerly (CLI + profile merge)
		string finalAssemblyPath;
		string finalSolutionName;
		string? finalClientId;
		string? finalTenantId;
		bool finalAllowEmptyTypes;

		if (profileName == null && !string.IsNullOrWhiteSpace(assemblyPath) && !string.IsNullOrWhiteSpace(solutionName))
		{
			// Standalone mode: all required values supplied via CLI
			finalAssemblyPath = assemblyPath;
			finalSolutionName = solutionName;
			finalClientId = clientId;
			finalTenantId = tenantId;
			finalAllowEmptyTypes = allowEmptyTypes ?? false;
		}
		else
		{
			// Profile mode: merge profile values with CLI overrides
			ProfileConfiguration? profile;
			try { profile = LoadProfileAndConfig(profileName).Profile; }
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
			finalClientId = clientId.GetValueOrDefault(pluginSyncItem?.ManagedIdentityClientId ?? string.Empty);
			finalTenantId = tenantId.GetValueOrDefault(pluginSyncItem?.ManagedIdentityTenantId ?? string.Empty);
			finalAllowEmptyTypes = allowEmptyTypes ?? pluginSyncItem?.AllowEmptyTypes ?? false;
		}

		return await RunCore(finalAssemblyPath, finalSolutionName, finalClientId, finalTenantId, finalAllowEmptyTypes, dryRun, ciMode, logLevel, profileName, cancellationToken);
	}

	private async Task<int> RunCore(
		string assemblyPath,
		string solutionName,
		string? managedIdentityClientId,
		string? managedIdentityTenantId,
		bool allowEmptyTypes,
		bool? dryRun,
		bool? ciMode,
		LogLevel? logLevel,
		string? profileName,
		CancellationToken ct)
	{
		var errors = XrmSyncConfigurationValidator.ValidateAssemblyPath(assemblyPath)
			.Concat(XrmSyncConfigurationValidator.ValidateSolutionName(solutionName))
			.ToList();

		// Both managed identity values must be present together when either is supplied
		var hasClientId = !string.IsNullOrWhiteSpace(managedIdentityClientId);
		var hasTenantId = !string.IsNullOrWhiteSpace(managedIdentityTenantId);
		if (hasClientId || hasTenantId)
		{
			errors.AddRange(XrmSyncConfigurationValidator.ValidateGuid(managedIdentityClientId ?? string.Empty, "Managed identity client ID"));
			errors.AddRange(XrmSyncConfigurationValidator.ValidateGuid(managedIdentityTenantId ?? string.Empty, "Managed identity tenant ID"));
		}

		if (errors.Count > 0)
			return ValidationError("plugins", errors);

		var serviceProvider = GetPluginSyncServices()
			.AddXrmSyncConfiguration(new ExecutionContext(null, null, null, null, profileName))
			.AddOptions(
				baseOptions => baseOptions with
				{
					LogLevel = logLevel ?? baseOptions.LogLevel,
					CiMode = ciMode ?? baseOptions.CiMode,
					DryRun = dryRun ?? baseOptions.DryRun
				})
			.AddSingleton(MSOptions.Create(new PluginSyncCommandOptions(assemblyPath, solutionName, managedIdentityClientId, managedIdentityTenantId, allowEmptyTypes)))
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
