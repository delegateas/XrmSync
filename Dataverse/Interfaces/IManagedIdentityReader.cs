using Microsoft.Xrm.Sdk;

namespace XrmSync.Dataverse.Interfaces;

public interface IManagedIdentityReader
{
	(Guid AssemblyId, EntityReference? ManagedIdentityRef)? GetPluginAssemblyManagedIdentity(Guid solutionId, string assemblyName);
}
