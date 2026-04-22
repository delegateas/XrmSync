using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.CommandLine;
using XrmSync.Constants;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Options;
using XrmSync.SyncService.Extensions;
using MSOptions = Microsoft.Extensions.Options.Options;

namespace XrmSync.Commands;

internal class IdentityCommand : XrmSyncSyncCommandBase
{
	private readonly Option<IdentityOperation?> operation;
	private readonly Option<string> assemblyFile;
	private readonly Option<string> clientId;
	private readonly Option<string> tenantId;

	// Root-level override options (advertised to XrmSyncRootCommand via GetProfileOverrides)
	private readonly Option<IdentityOperation?> rootOperation = CliOptions.ManagedIdentity.Operation.CreateOption<IdentityOperation?>();
	private readonly Option<string?> rootClientId = CliOptions.ManagedIdentity.ClientId.CreateOption<string?>();
	private readonly Option<string?> rootTenantId = CliOptions.ManagedIdentity.TenantId.CreateOption<string?>();

	public IdentityCommand() : base("identity", "Manage the managed identity linked to a plugin assembly")
	{
		operation = CliOptions.ManagedIdentity.Operation.CreateOption<IdentityOperation?>();
		assemblyFile = CliOptions.Assembly.CreateOption<string>();
		clientId = CliOptions.ManagedIdentity.ClientId.CreateOption<string>();
		tenantId = CliOptions.ManagedIdentity.TenantId.CreateOption<string>();

		Add(operation);
		Add(assemblyFile);
		Add(clientId);
		Add(tenantId);

		AddSharedOptions();
		AddSyncSharedOptions();

		SetAction(ExecuteAsync);
	}

	/// <summary>
	/// Advertises --client-id and --tenant-id as root-level overrides.
	/// The shared assembly option is used in the merge callback but owned by the root command.
	/// </summary>
	public override ProfileOverrideProvider? GetProfileOverrides(Option<string?> assembly, Option<string?> solution) => new(
		options: [rootOperation, rootClientId, rootTenantId],
		mergeSyncItem: (item, parseResult) =>
		{
			if (item is not IdentitySyncItem identity) return null;
			var operationValue = parseResult.GetValue(rootOperation);
			var clientIdValue = parseResult.GetValue(rootClientId);
			var tenantIdValue = parseResult.GetValue(rootTenantId);
			var assemblyValue = parseResult.GetValue(assembly);
			return identity with
			{
				Operation = operationValue ?? identity.Operation,
				AssemblyPath = !string.IsNullOrWhiteSpace(assemblyValue) ? assemblyValue : identity.AssemblyPath,
				ClientId = !string.IsNullOrWhiteSpace(clientIdValue) ? clientIdValue : identity.ClientId,
				TenantId = !string.IsNullOrWhiteSpace(tenantIdValue) ? tenantIdValue : identity.TenantId
			};
		});

	private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
	{
		var operationValue = parseResult.GetValue(operation);
		var assemblyPath = parseResult.GetValue(assemblyFile);
		var clientIdValue = parseResult.GetValue(clientId);
		var tenantIdValue = parseResult.GetValue(tenantId);
		var (solutionName, dryRun, logLevel, ciMode) = GetSyncSharedOptionValues(parseResult);
		var sharedOptions = GetSharedOptionValues(parseResult);

		// Resolve final options eagerly (CLI + profile merge)
		IdentityOperation? finalOperation;
		string finalAssemblyPath;
		string finalSolutionName;
		string? finalClientId;
		string? finalTenantId;

		if (sharedOptions.ProfileName == null && !string.IsNullOrWhiteSpace(assemblyPath) && !string.IsNullOrWhiteSpace(solutionName))
		{
			// Standalone mode: all required values supplied via CLI
			finalOperation = operationValue;
			finalAssemblyPath = assemblyPath;
			finalSolutionName = solutionName;
			finalClientId = clientIdValue;
			finalTenantId = tenantIdValue;
		}
		else
		{
			// Profile mode: merge profile values with CLI overrides
			ProfileConfiguration? profile;
			try { profile = LoadProfile(sharedOptions.ProfileName); }
			catch (XrmSyncException ex) { Console.Error.WriteLine(ex.Message); return E_ERROR; }

			if (profile == null)
			{
				Console.Error.WriteLine("No profiles configured. Specify --assembly and --solution, or add a profile to appsettings.json.");
				return E_ERROR;
			}

			// Sync item is optional — if absent, CLI must supply all identity-specific values.
			// Priority: exact operation match → null-operation item as fallback → any item when no operation is specified.
			var identityItems = profile.Sync.OfType<IdentitySyncItem>().ToList();
			var syncItem = operationValue.HasValue
				? identityItems.FirstOrDefault(i => i.Operation == operationValue)
					?? identityItems.FirstOrDefault(i => i.Operation == null)
				: identityItems.FirstOrDefault();

			finalOperation = operationValue ?? syncItem?.Operation;
			finalAssemblyPath = !string.IsNullOrWhiteSpace(assemblyPath) ? assemblyPath : (syncItem?.AssemblyPath ?? string.Empty);
			finalSolutionName = !string.IsNullOrWhiteSpace(solutionName) ? solutionName : profile.SolutionName;
			finalClientId = !string.IsNullOrWhiteSpace(clientIdValue) ? clientIdValue : syncItem?.ClientId;
			finalTenantId = !string.IsNullOrWhiteSpace(tenantIdValue) ? tenantIdValue : syncItem?.TenantId;
		}

		// Validate resolved values
		var errors = new List<string>();

		if (finalOperation == null)
			errors.Add("Operation is required. Specify 'Remove' or 'Ensure' via --operation.");

		errors.AddRange(XrmSyncConfigurationValidator.ValidateAssemblyPath(finalAssemblyPath));
		errors.AddRange(XrmSyncConfigurationValidator.ValidateSolutionName(finalSolutionName));

		if (finalOperation == IdentityOperation.Ensure)
		{
			errors.AddRange(XrmSyncConfigurationValidator.ValidateGuid(finalClientId ?? string.Empty, "Client ID"));
			errors.AddRange(XrmSyncConfigurationValidator.ValidateGuid(finalTenantId ?? string.Empty, "Tenant ID"));
		}

		if (errors.Count > 0)
			return ValidationError($"identity --operation {finalOperation?.ToString() ?? "<none>"}", errors);

		// Build service provider with validated options
		var serviceProvider = new ServiceCollection()
			.AddIdentityService()
			.AddXrmSyncConfiguration(sharedOptions)
			.AddOptions(
				baseOptions => baseOptions with
				{
					LogLevel = logLevel ?? baseOptions.LogLevel,
					CiMode = ciMode ?? baseOptions.CiMode,
					DryRun = dryRun ?? baseOptions.DryRun
				})
			.AddSingleton(MSOptions.Create(new IdentityCommandOptions(finalOperation!.Value, finalAssemblyPath, finalSolutionName, finalClientId, finalTenantId)))
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
}
