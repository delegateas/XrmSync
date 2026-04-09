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
	private readonly Option<IdentityOperation> operation;
	private readonly Option<string> assemblyFile;
	private readonly Option<string> clientId;
	private readonly Option<string> tenantId;

	public IdentityCommand() : base("identity", "Manage the managed identity linked to a plugin assembly")
	{
		operation = new(CliOptions.ManagedIdentity.Operation.Primary, CliOptions.ManagedIdentity.Operation.Aliases)
		{
			Description = CliOptions.ManagedIdentity.Operation.Description,
			Arity = ArgumentArity.ExactlyOne,
			Required = true
		};

		assemblyFile = new(CliOptions.Assembly.Primary, CliOptions.Assembly.Aliases)
		{
			Description = CliOptions.Assembly.Description,
			Arity = ArgumentArity.ZeroOrOne
		};

		clientId = new(CliOptions.ManagedIdentity.ClientId.Primary, CliOptions.ManagedIdentity.ClientId.Aliases)
		{
			Description = CliOptions.ManagedIdentity.ClientId.Description,
			Arity = ArgumentArity.ZeroOrOne
		};

		tenantId = new(CliOptions.ManagedIdentity.TenantId.Primary, CliOptions.ManagedIdentity.TenantId.Aliases)
		{
			Description = CliOptions.ManagedIdentity.TenantId.Description,
			Arity = ArgumentArity.ZeroOrOne
		};

		Add(operation);
		Add(assemblyFile);
		Add(clientId);
		Add(tenantId);

		AddSharedOptions();
		AddSyncSharedOptions();

		SetAction(ExecuteAsync);
	}

	private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
	{
		var operationValue = parseResult.GetValue(operation);
		var assemblyPath = parseResult.GetValue(assemblyFile);
		var clientIdValue = parseResult.GetValue(clientId);
		var tenantIdValue = parseResult.GetValue(tenantId);
		var (solutionName, dryRun, logLevel, ciMode) = GetSyncSharedOptionValues(parseResult);
		var sharedOptions = GetSharedOptionValues(parseResult);

		// Resolve final options eagerly (CLI + profile merge)
		IdentityOperation finalOperation;
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

			var syncItem = profile?.Sync.OfType<IdentitySyncItem>().FirstOrDefault(i => i.Operation == operationValue);
			if (profile == null || syncItem == null)
			{
				Console.Error.WriteLine(
					profile == null
						? "No profiles configured. Specify --assembly and --solution, or add a profile to appsettings.json."
						: $"Profile '{profile.Name}' does not contain an Identity {operationValue} sync item. Specify --assembly and --solution, or add a matching Identity sync item to the profile.");
				return E_ERROR;
			}

			finalOperation = syncItem.Operation;
			finalAssemblyPath = !string.IsNullOrWhiteSpace(assemblyPath) ? assemblyPath : syncItem.AssemblyPath;
			finalSolutionName = !string.IsNullOrWhiteSpace(solutionName) ? solutionName : profile.SolutionName;
			finalClientId = !string.IsNullOrWhiteSpace(clientIdValue) ? clientIdValue : syncItem.ClientId;
			finalTenantId = !string.IsNullOrWhiteSpace(tenantIdValue) ? tenantIdValue : syncItem.TenantId;
		}

		// Validate resolved values
		var errors = XrmSyncConfigurationValidator.ValidateAssemblyPath(finalAssemblyPath)
			.Concat(XrmSyncConfigurationValidator.ValidateSolutionName(finalSolutionName))
			.ToList();

		if (finalOperation == IdentityOperation.Ensure)
		{
			errors.AddRange(XrmSyncConfigurationValidator.ValidateGuid(finalClientId ?? string.Empty, "Client ID"));
			errors.AddRange(XrmSyncConfigurationValidator.ValidateGuid(finalTenantId ?? string.Empty, "Tenant ID"));
		}

		if (errors.Count > 0)
			return ValidationError($"identity --operation {finalOperation}", errors);

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
			.AddSingleton(MSOptions.Create(new IdentityCommandOptions(finalOperation, finalAssemblyPath, finalSolutionName, finalClientId, finalTenantId)))
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
