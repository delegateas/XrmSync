using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Model.Plugin;
using XrmSync.Options;
using XrmSync.SyncService.Extensions;
using XrmSync.Watch;
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
		Add(CommandOptions.NoDelete);
		Add(CommandOptions.Watch);

		AddSharedOptions();
		AddSyncOptions();

		SetAction(ExecuteAsync);
	}

	public override async Task<int?> ExecuteFromProfile(SyncItem syncItem, ExecutionContext ctx, CancellationToken ct)
	{
		if (syncItem is not PluginSyncItem plugin) return null;
		return await RunCore(plugin.AssemblyPath ?? string.Empty, ctx.SolutionName ?? string.Empty, plugin.ManagedIdentityClientId, plugin.ManagedIdentityTenantId, plugin.AllowEmptyTypes, plugin.NoDelete, ctx.DryRun, ctx.CiMode, ctx.LogLevel, ctx.ProfileName, ct);
	}

	private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
	{
		var assemblyPath = parseResult.GetValue(CommandOptions.Assembly);
		var solutionName = parseResult.GetValue(CommandOptions.Solution);
		var clientId = parseResult.GetValue(CommandOptions.ClientId);
		var tenantId = parseResult.GetValue(CommandOptions.TenantId);
		var allowEmptyTypes = parseResult.GetValue(CommandOptions.AllowEmptyTypes);
		var noDelete = parseResult.GetValue(CommandOptions.NoDelete);
		var watch = parseResult.GetValue(CommandOptions.Watch);
		var (dryRun, ciMode, logLevel, profileName) = ReadExecutionOverrides(parseResult);

		var (profile, exitCode) = ResolveCommandProfile(profileName,
			!string.IsNullOrWhiteSpace(assemblyPath) && !string.IsNullOrWhiteSpace(solutionName),
			"Specify --assembly and --solution, or add a profile to appsettings.json.");
		if (exitCode.HasValue) return exitCode.Value;

		// Sync item is optional — its assembly path and solution name fall back to the profile-level shared values
		var item = profile?.Sync.OfType<PluginSyncItem>().FirstOrDefault();

		var finalAssemblyPath = assemblyPath.GetValueOrDefault(profile?.ResolveAssemblyPath(item?.AssemblyPath) ?? string.Empty);
		var finalSolutionName = solutionName.GetValueOrDefault(profile?.ResolveSolutionName(item) ?? string.Empty);
		var finalClientId = clientId.GetValueOrDefault(item?.ManagedIdentityClientId ?? string.Empty);
		var finalTenantId = tenantId.GetValueOrDefault(item?.ManagedIdentityTenantId ?? string.Empty);
		var finalAllowEmptyTypes = allowEmptyTypes ?? item?.AllowEmptyTypes ?? false;
		var finalNoDelete = noDelete ?? item?.NoDelete ?? false;

		var watchSettings = ResolveWatchSettings(watch, item?.Watch ?? false, ciMode);

		var initialResult = await RunCore(finalAssemblyPath, finalSolutionName, finalClientId, finalTenantId, finalAllowEmptyTypes, finalNoDelete, dryRun, ciMode, logLevel, profileName, cancellationToken);

		if (!watchSettings.Enabled)
			return initialResult;

		var target = WatchTargetResolver.ForAssembly(finalAssemblyPath, item ?? PluginSyncItem.Empty);
		if (target == null)
			return initialResult;

		await CreateWatchLoop(watchSettings, dryRun, ciMode, logLevel, profileName)
			.RunAsync([target], (_, ct) => RunCore(finalAssemblyPath, finalSolutionName, finalClientId, finalTenantId, finalAllowEmptyTypes, finalNoDelete, dryRun, ciMode, logLevel, profileName, ct), cancellationToken);

		return initialResult;
	}

	private async Task<int> RunCore(
		string assemblyPath,
		string solutionName,
		string? managedIdentityClientId,
		string? managedIdentityTenantId,
		bool allowEmptyTypes,
		bool noDelete,
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
			.AddOptions(dryRun, ciMode, logLevel)
			.AddSingleton(MSOptions.Create(new PluginSyncCommandOptions(assemblyPath, solutionName, managedIdentityClientId, managedIdentityTenantId, allowEmptyTypes, noDelete)))
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
