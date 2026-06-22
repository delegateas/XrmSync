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
			CredentialSource = credentialsource.IsManaged,
			SubjectScope = subjectscope.EnviornmentScope,
			Version = 1
		}, null);
	}

	public void Update(Guid managedIdentityId, string name, Guid applicationId, Guid tenantId)
	{
		writer.Update(new ManagedIdentity(managedIdentityId)
		{
			Name = name,
			ApplicationId = applicationId,
			TenantId = tenantId
		});
	}

	public void LinkToAssembly(Guid assemblyId, Guid managedIdentityId)
	{
		writer.Update(new PluginAssembly(assemblyId)
		{
			ManagedIdentityId = new EntityReference(ManagedIdentity.EntityLogicalName, managedIdentityId)
		});
	}
}
