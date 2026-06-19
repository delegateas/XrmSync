using Microsoft.Xrm.Sdk;
using XrmSync.Model.Identity;

namespace XrmSync.Dataverse.Interfaces;

public interface IManagedIdentityReader
{
	(Guid AssemblyId, EntityReference? ManagedIdentityRef)? GetPluginAssemblyManagedIdentity(Guid solutionId, string assemblyName);

	/// <summary>
	/// Retrieves the currently registered state of a managed identity record, or null when it no longer exists.
	/// </summary>
	ManagedIdentityInfo? GetManagedIdentity(Guid managedIdentityId);
}
