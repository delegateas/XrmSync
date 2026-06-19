using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Model.Identity;

namespace XrmSync.SyncService;

internal class IdentitySyncService(
	ISolutionReader solutionReader,
	IManagedIdentityReader managedIdentityReader,
	IManagedIdentityReconciler managedIdentityService,
	IOptions<IdentityCommandOptions> configuration,
	ILogger<IdentitySyncService> log) : ISyncService
{
	private readonly IdentityCommandOptions options = configuration.Value;

	public Task Sync(CancellationToken cancellation)
	{
		log.LogInformation("{operation} managed identity for assembly '{assemblyName}'",
			options.Operation, Path.GetFileNameWithoutExtension(options.AssemblyPath));

		return options.Operation switch
		{
			IdentityOperation.Remove => Remove(),
			IdentityOperation.Ensure => Ensure(),
			_ => throw new XrmSyncException($"Unknown identity operation: {options.Operation}")
		};
	}

	private Task Remove()
	{
		var assemblyName = Path.GetFileNameWithoutExtension(options.AssemblyPath);
		log.LogInformation("Removing managed identity for assembly '{assemblyName}' in solution '{solutionName}'",
			assemblyName, options.SolutionName);

		var (solutionId, _) = solutionReader.RetrieveSolution(options.SolutionName);
		var result = managedIdentityReader.GetPluginAssemblyManagedIdentity(solutionId, assemblyName);

		if (result == null)
		{
			// A missing assembly must not block removal — there is nothing to clean up.
			log.LogWarning("Plugin assembly '{assemblyName}' not found in solution '{solutionName}'. Nothing to remove.",
				assemblyName, options.SolutionName);
			return Task.CompletedTask;
		}

		managedIdentityService.Remove(result.Value.ManagedIdentityRef, assemblyName);
		return Task.CompletedTask;
	}

	private Task Ensure()
	{
		var assemblyName = Path.GetFileNameWithoutExtension(options.AssemblyPath);
		log.LogInformation("Ensuring managed identity for assembly '{assemblyName}' in solution '{solutionName}'",
			assemblyName, options.SolutionName);

		if (!Guid.TryParse(options.ClientId, out var clientId))
			throw new XrmSyncException(string.IsNullOrWhiteSpace(options.ClientId)
				? "Client ID is required and cannot be empty."
				: "Client ID must be a valid GUID.");

		if (!Guid.TryParse(options.TenantId, out var tenantId))
			throw new XrmSyncException(string.IsNullOrWhiteSpace(options.TenantId)
				? "Tenant ID is required and cannot be empty."
				: "Tenant ID must be a valid GUID.");

		var (solutionId, _) = solutionReader.RetrieveSolution(options.SolutionName);
		var result = managedIdentityReader.GetPluginAssemblyManagedIdentity(solutionId, assemblyName);

		if (result == null)
			throw new XrmSyncException($"Plugin assembly '{assemblyName}' not found in solution '{options.SolutionName}'.");

		var (assemblyId, managedIdentityRef) = result.Value;

		managedIdentityService.Ensure(assemblyId, managedIdentityRef, options.SolutionName, clientId, tenantId);

		log.LogInformation("Successfully ensured managed identity for assembly '{assemblyName}'", assemblyName);
		return Task.CompletedTask;
	}
}
