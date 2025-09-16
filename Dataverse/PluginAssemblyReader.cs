using Microsoft.PowerPlatform.Dataverse.Client;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;

namespace XrmSync.Dataverse;

public class PluginAssemblyReader(ServiceClient serviceClient) : IPluginAssemblyReader
{
    public AssemblyInfo? GetPluginAssembly(Guid solutionId, string assemblyName)
    {
        using var xrm = new DataverseContext(serviceClient);

        return (from pa in xrm.PluginAssemblySet
                join sc in xrm.SolutionComponentSet on pa.Id equals sc.ObjectId
                where sc.SolutionId != null && sc.SolutionId.Id == solutionId && pa.Name == assemblyName
                select new AssemblyInfo
                {
                    Id = pa.Id,
                    Name = pa.Name ?? string.Empty,
                    Version = pa.Version ?? string.Empty,
                    Hash = pa.SourceHash ?? string.Empty,
                }).FirstOrDefault();
    }
}
