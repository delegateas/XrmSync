using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.SyncService.Difference;

namespace XrmSync.SyncService;

internal class IdentitySyncService(
	ISolutionReader solutionReader,
	IManagedIdentityReader managedIdentityReader,
	IManagedIdentityWriter managedIdentityWriter,
	IOptions<IdentityCommandOptions> configuration,
	IPrintService printService,
	ILogger<IdentitySyncService> log) : ISyncService
{
	private readonly IdentityCommandOptions options = configuration.Value;

	public Task Sync(CancellationToken cancellation)
	{
		printService.PrintHeader(PrintHeaderOptions.Default with
		{
			Message = $"{options.Operation} managed identity for assembly '{Path.GetFileNameWithoutExtension(options.AssemblyPath)}'"
		});

		return options.Operation switch
		{
			IdentityOperation.Remove => Remove(cancellation),
			IdentityOperation.Ensure => Ensure(cancellation),
			_ => throw new XrmSyncException($"Unknown identity operation: {options.Operation}")
		};
	}

	private Task Remove(CancellationToken cancellation)
	{
		var assemblyName = Path.GetFileNameWithoutExtension(options.AssemblyPath);
		log.LogInformation("Removing managed identity for assembly '{assemblyName}' in solution '{solutionName}'",
			assemblyName, options.SolutionName);

		var (solutionId, _) = solutionReader.RetrieveSolution(options.SolutionName);
		var result = managedIdentityReader.GetPluginAssemblyManagedIdentity(solutionId, assemblyName);

		if (result == null)
			throw new XrmSyncException($"Plugin assembly '{assemblyName}' not found in solution '{options.SolutionName}'.");

		var (_, managedIdentityRef) = result.Value;

		if (managedIdentityRef == null)
		{
			log.LogWarning("No managed identity linked to assembly '{assemblyName}'. Nothing to remove.", assemblyName);
			return Task.CompletedTask;
		}

		log.LogInformation("Deleting managed identity '{managedIdentityId}' linked to assembly '{assemblyName}'",
			managedIdentityRef.Id, assemblyName);

		managedIdentityWriter.Remove(managedIdentityRef.Id);

		log.LogInformation("Successfully removed managed identity from assembly '{assemblyName}'", assemblyName);
		return Task.CompletedTask;
	}

	private Task Ensure(CancellationToken cancellation)
	{
		var assemblyName = Path.GetFileNameWithoutExtension(options.AssemblyPath);
		log.LogInformation("Ensuring managed identity for assembly '{assemblyName}' in solution '{solutionName}'",
			assemblyName, options.SolutionName);

		if (!Guid.TryParse(options.ClientId, out var clientId) || !Guid.TryParse(options.TenantId, out var tenantId))
			throw new XrmSyncException("ClientId and TenantId must be valid GUIDs for the Ensure operation.");

		var (solutionId, _) = solutionReader.RetrieveSolution(options.SolutionName);
		var result = managedIdentityReader.GetPluginAssemblyManagedIdentity(solutionId, assemblyName);

		if (result == null)
			throw new XrmSyncException($"Plugin assembly '{assemblyName}' not found in solution '{options.SolutionName}'.");

		var (assemblyId, managedIdentityRef) = result.Value;

		if (managedIdentityRef != null)
		{
			log.LogInformation("Managed identity already linked to assembly '{assemblyName}'. No action needed.", assemblyName);
			return Task.CompletedTask;
		}

		var miName = $"{options.SolutionName} Managed Identity";
		log.LogInformation("Creating managed identity '{miName}' for assembly '{assemblyName}'", miName, assemblyName);

		var managedIdentityId = managedIdentityWriter.Create(miName, clientId, tenantId);

		log.LogInformation("Linking managed identity '{managedIdentityId}' to assembly '{assemblyName}'",
			managedIdentityId, assemblyName);

		managedIdentityWriter.LinkToAssembly(assemblyId, managedIdentityId);

		log.LogInformation("Successfully ensured managed identity for assembly '{assemblyName}'", assemblyName);
		return Task.CompletedTask;
	}
}
