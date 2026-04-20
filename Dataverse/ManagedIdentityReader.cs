using Microsoft.Xrm.Sdk;
using XrmSync.Dataverse.Interfaces;

namespace XrmSync.Dataverse;

internal class ManagedIdentityReader(IDataverseReader reader) : IManagedIdentityReader
{
	public (Guid AssemblyId, EntityReference? ManagedIdentityRef)? GetPluginAssemblyManagedIdentity(Guid solutionId, string assemblyName)
	{
		return (from pa in reader.PluginAssemblies
				join sc in reader.SolutionComponents on pa.Id equals sc.ObjectId
				where sc.SolutionId != null && sc.SolutionId.Id == solutionId && pa.Name == assemblyName
				select new
				{
					pa.Id,
					pa.ManagedIdentityId
				}).FirstOrDefault() is { } result
			? (result.Id, result.ManagedIdentityId)
			: null;
	}
}
