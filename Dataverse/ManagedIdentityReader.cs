using Microsoft.Xrm.Sdk;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.Identity;

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

	public ManagedIdentityInfo? GetManagedIdentity(Guid managedIdentityId)
	{
		return (from mi in reader.ManagedIdentities
				where mi.Id == managedIdentityId
				select new ManagedIdentityInfo(mi.Id, mi.Name, mi.ApplicationId, mi.TenantId))
			.FirstOrDefault();
	}
}
