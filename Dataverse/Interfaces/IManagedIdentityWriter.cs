namespace XrmSync.Dataverse.Interfaces;

public interface IManagedIdentityWriter
{
	void Remove(Guid managedIdentityId);
	Guid Create(string name, Guid applicationId, Guid tenantId);
	void LinkToAssembly(Guid assemblyId, Guid managedIdentityId);
}
