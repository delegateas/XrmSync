using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using XrmSync.Dataverse.Interfaces;

namespace XrmSync.SyncService;

/// <summary>
/// Shared managed identity reconciliation used by both the standalone identity command
/// and the integrated plugin sync. Ensures (upserts) and removes the managed identity
/// bound to a plugin assembly.
/// </summary>
internal interface IManagedIdentityReconciler
{
	/// <summary>
	/// Ensures the managed identity linked to <paramref name="assemblyId"/> matches the desired
	/// application/tenant. Creates and links a new identity when none exists, or updates the
	/// existing record in place when its application id, tenant id or name has drifted.
	/// </summary>
	void Ensure(Guid assemblyId, EntityReference? current, string solutionName, Guid clientId, Guid tenantId);

	/// <summary>
	/// Removes the managed identity currently linked to an assembly. Logs a warning and does
	/// nothing when no identity is linked.
	/// </summary>
	void Remove(EntityReference? current, string assemblyName);
}

internal class ManagedIdentityReconciler(
	IManagedIdentityReader reader,
	IManagedIdentityWriter writer,
	ILogger<ManagedIdentityReconciler> log) : IManagedIdentityReconciler
{
	public void Ensure(Guid assemblyId, EntityReference? current, string solutionName, Guid clientId, Guid tenantId)
	{
		var name = $"{solutionName} Managed Identity";

		if (current == null)
		{
			log.LogInformation("Creating managed identity '{name}'", name);
			var managedIdentityId = writer.Create(name, clientId, tenantId);

			log.LogInformation("Linking managed identity '{managedIdentityId}' to assembly", managedIdentityId);
			writer.LinkToAssembly(assemblyId, managedIdentityId);
			return;
		}

		var existing = reader.GetManagedIdentity(current.Id);
		if (existing == null)
		{
			// The lookup pointed at a record that no longer exists — recreate and relink.
			log.LogWarning("Linked managed identity '{id}' could not be read; creating a replacement", current.Id);
			var managedIdentityId = writer.Create(name, clientId, tenantId);
			writer.LinkToAssembly(assemblyId, managedIdentityId);
			return;
		}

		if (existing.ApplicationId == clientId && existing.TenantId == tenantId && existing.Name == name)
		{
			log.LogInformation("Managed identity '{name}' is already up to date; no changes needed", name);
			return;
		}

		log.LogInformation("Updating managed identity '{name}' to match the configured application and tenant", name);
		writer.Update(current.Id, name, clientId, tenantId);
	}

	public void Remove(EntityReference? current, string assemblyName)
	{
		if (current == null)
		{
			log.LogWarning("No managed identity linked to assembly '{assemblyName}'. Nothing to remove.", assemblyName);
			return;
		}

		// EntityReference.Name is usually populated for lookup columns, but can be blank if the
		// related record has no primary name — fall back to the id so the log stays actionable.
		var identityLabel = string.IsNullOrWhiteSpace(current.Name) ? current.Id.ToString() : current.Name;
		log.LogInformation("Deleting managed identity '{managedIdentityName}' linked to assembly '{assemblyName}'",
			identityLabel, assemblyName);
		writer.Remove(current.Id);

		log.LogInformation("Successfully removed managed identity from assembly '{assemblyName}'", assemblyName);
	}
}
