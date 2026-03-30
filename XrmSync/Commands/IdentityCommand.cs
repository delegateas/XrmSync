using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.CommandLine;
using XrmSync.Constants;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Options;
using XrmSync.SyncService.Extensions;

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
			.AddSingleton(sp =>
			{
				IdentityOperation finalOperation;
				string finalAssemblyPath;
				string finalSolutionName;
				string? finalClientId;
				string? finalTenantId;

				if (!string.IsNullOrWhiteSpace(assemblyPath) && !string.IsNullOrWhiteSpace(solutionName))
				{
					finalOperation = operationValue;
					finalAssemblyPath = assemblyPath;
					finalSolutionName = solutionName;
					finalClientId = clientIdValue;
					finalTenantId = tenantIdValue;

					if (finalOperation == IdentityOperation.Ensure)
					{
						var errors = new List<string>();
						if (string.IsNullOrWhiteSpace(finalClientId))
							errors.Add("Client ID is required and cannot be empty.");
						else if (!Guid.TryParse(finalClientId, out _))
							errors.Add("Client ID must be a valid GUID.");

						if (string.IsNullOrWhiteSpace(finalTenantId))
							errors.Add("Tenant ID is required and cannot be empty.");
						else if (!Guid.TryParse(finalTenantId, out _))
							errors.Add("Tenant ID must be a valid GUID.");

						if (errors.Count > 0)
							throw new Model.Exceptions.OptionsValidationException("identity --operation Ensure", errors);
					}
				}
				else
				{
					var profile = GetRequiredProfile(sp, sharedOptions.ProfileName, "--assembly and --solution");

					var syncItem = profile.Sync.OfType<IdentitySyncItem>().FirstOrDefault(i => i.Operation == operationValue)
						?? throw new InvalidOperationException(
							$"Profile '{profile.Name}' does not contain an Identity {operationValue} sync item. " +
							"Either specify --assembly and --solution, or use a profile with a matching Identity sync item.");

					finalOperation = syncItem.Operation;
					finalAssemblyPath = !string.IsNullOrWhiteSpace(assemblyPath)
						? assemblyPath
						: syncItem.AssemblyPath;
					finalSolutionName = !string.IsNullOrWhiteSpace(solutionName)
						? solutionName
						: profile.SolutionName;
					finalClientId = !string.IsNullOrWhiteSpace(clientIdValue)
						? clientIdValue
						: syncItem.ClientId;
					finalTenantId = !string.IsNullOrWhiteSpace(tenantIdValue)
						? tenantIdValue
						: syncItem.TenantId;
				}

				return Microsoft.Extensions.Options.Options.Create(
					new IdentityCommandOptions(finalOperation, finalAssemblyPath, finalSolutionName, finalClientId, finalTenantId));
			})
			.AddSingleton(sp =>
			{
				var config = sp.GetRequiredService<IOptions<XrmSyncConfiguration>>().Value;
				return Microsoft.Extensions.Options.Options.Create(new ExecutionModeOptions(config.DryRun));
			})
			.AddLogger()
			.BuildServiceProvider();

		return await RunAction(serviceProvider, ConfigurationScope.Identity, CommandAction, cancellationToken)
			? E_OK
			: E_ERROR;
	}
}
