using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.CommandLine;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Model.Identity;
using XrmSync.Options;
using XrmSync.SyncService.Extensions;
using MSOptions = Microsoft.Extensions.Options.Options;

namespace XrmSync.Commands;

internal class IdentityCommand : XrmSyncCommandBase
{
	public IdentityCommand() : base("identity", "Manage the managed identity linked to a plugin assembly")
	{
		Add(CommandOptions.Operation);
		Add(CommandOptions.Assembly);
		Add(CommandOptions.ClientId);
		Add(CommandOptions.TenantId);

		AddSharedOptions();
		AddSyncOptions();

		SetAction(ExecuteAsync);
	}

	public override async Task<int?> ExecuteFromProfile(SyncItem syncItem, ExecutionContext ctx, CancellationToken ct)
	{
		if (syncItem is not IdentitySyncItem identity) return null;

		if (identity.Operation == null)
		{
			Console.Error.WriteLine("Identity sync item has no operation configured and none was supplied via --operation.");
			return E_ERROR;
		}

		return await RunCore(
			identity.Operation.Value,
			identity.AssemblyPath ?? string.Empty,
			ctx.SolutionName ?? string.Empty,
			identity.ClientId,
			identity.TenantId,
			ctx.DryRun,
			ctx.CiMode,
			ctx.LogLevel,
			ctx.ProfileName,
			ct);
	}

	private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
	{
		var operationValue = parseResult.GetValue(CommandOptions.Operation);
		var assemblyPath = parseResult.GetValue(CommandOptions.Assembly);
		var clientIdValue = parseResult.GetValue(CommandOptions.ClientId);
		var tenantIdValue = parseResult.GetValue(CommandOptions.TenantId);
		var solutionName = parseResult.GetValue(CommandOptions.Solution);
		var (dryRun, ciMode, logLevel, profileName) = ReadExecutionOverrides(parseResult);

		var (profile, exitCode) = ResolveCommandProfile(profileName,
			!string.IsNullOrWhiteSpace(assemblyPath) && !string.IsNullOrWhiteSpace(solutionName),
			"Specify --assembly and --solution, or add a profile to appsettings.json.");
		if (exitCode.HasValue) return exitCode.Value;

		// Sync item is optional — its assembly path and solution name fall back to the profile-level shared values.
		// Priority: exact operation match → null-operation item as fallback → any item when no operation is specified.
		var identityItems = profile?.Sync.OfType<IdentitySyncItem>().ToList() ?? [];
		var item = operationValue.HasValue
			? identityItems.FirstOrDefault(i => i.Operation == operationValue)
				?? identityItems.FirstOrDefault(i => i.Operation == null)
			: identityItems.FirstOrDefault();

		var finalOperation = operationValue ?? item?.Operation;
		var finalAssemblyPath = assemblyPath.GetValueOrDefault(profile?.ResolveAssemblyPath(item?.AssemblyPath) ?? string.Empty);
		var finalSolutionName = solutionName.GetValueOrDefault(profile?.ResolveSolutionName(item) ?? string.Empty);
		var finalClientId = clientIdValue.GetValueOrDefault(item?.ClientId ?? string.Empty);
		var finalTenantId = tenantIdValue.GetValueOrDefault(item?.TenantId ?? string.Empty);

		// Validate resolved values
		var errors = new List<string>();

		if (finalOperation == null)
			errors.Add("Operation is required. Specify 'Remove' or 'Ensure' via --operation.");

		errors.AddRange(XrmSyncConfigurationValidator.ValidateAssemblyPath(finalAssemblyPath));
		errors.AddRange(XrmSyncConfigurationValidator.ValidateSolutionName(finalSolutionName));

		if (finalOperation == IdentityOperation.Ensure)
		{
			errors.AddRange(XrmSyncConfigurationValidator.ValidateGuid(finalClientId, "Client ID"));
			errors.AddRange(XrmSyncConfigurationValidator.ValidateGuid(finalTenantId, "Tenant ID"));
		}

		if (errors.Count > 0)
			return ValidationError($"identity --operation {finalOperation?.ToString() ?? "<none>"}", errors);

		return await RunCore(finalOperation!.Value, finalAssemblyPath, finalSolutionName, finalClientId, finalTenantId, dryRun, ciMode, logLevel, profileName, cancellationToken);
	}

	private async Task<int> RunCore(
		IdentityOperation operation,
		string assemblyPath,
		string solutionName,
		string clientId,
		string tenantId,
		bool? dryRun,
		bool? ciMode,
		LogLevel? logLevel,
		string? profileName,
		CancellationToken ct)
	{
		// Validate resolved values (when called from ExecuteAsync, validation already done above)
		var errors = new List<string>();
		errors.AddRange(XrmSyncConfigurationValidator.ValidateAssemblyPath(assemblyPath));
		errors.AddRange(XrmSyncConfigurationValidator.ValidateSolutionName(solutionName));

		if (operation == IdentityOperation.Ensure)
		{
			errors.AddRange(XrmSyncConfigurationValidator.ValidateGuid(clientId, "Client ID"));
			errors.AddRange(XrmSyncConfigurationValidator.ValidateGuid(tenantId, "Tenant ID"));
		}

		if (errors.Count > 0)
			return ValidationError($"identity --operation {operation}", errors);

		var serviceProvider = new ServiceCollection()
			.AddIdentityService()
			.AddXrmSyncConfiguration(new ExecutionContext(null, null, null, null, profileName))
			.AddOptions(dryRun, ciMode, logLevel)
			.AddSingleton(MSOptions.Create(new IdentityCommandOptions(operation, assemblyPath, solutionName, clientId, tenantId)))
			.AddLogger()
			.BuildServiceProvider();

		return await RunAction(serviceProvider, ConfigurationScope.None, SyncCommandAction, ct)
			? E_OK
			: E_ERROR;
	}
}
