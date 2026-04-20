using Microsoft.Xrm.Sdk;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Interfaces;

namespace XrmSync.Dataverse;

internal class ManagedIdentityWriter(IDataverseWriter writer) : IManagedIdentityWriter
{
	public void Remove(Guid managedIdentityId)
	{
		writer.Delete(new ManagedIdentity(managedIdentityId));
	}

	public Guid Create(string name, Guid applicationId, Guid tenantId)
	{
		return writer.Create(new ManagedIdentity
		{
			Name = name,
			ApplicationId = applicationId,
			TenantId = tenantId,
			CredentialSource = managedidentity_credentialsource.ManagedIdentity,
			SubjectScope = managedidentity_subjectscope.Environment,
			ManagedIdentityVersion = 1
		}, null);
	}

	public void LinkToAssembly(Guid assemblyId, Guid managedIdentityId)
	{
		writer.Update(new PluginAssembly(assemblyId)
		{
			ManagedIdentityId = new EntityReference(ManagedIdentity.EntityLogicalName, managedIdentityId)
		});
	}
}
