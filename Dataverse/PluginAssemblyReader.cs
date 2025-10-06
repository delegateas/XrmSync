using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;

namespace XrmSync.Dataverse;

public class PluginAssemblyReader(IDataverseReader reader) : IPluginAssemblyReader
{
    public AssemblyInfo? GetPluginAssembly(Guid solutionId, string assemblyName)
    {
        return (from pa in reader.PluginAssemblies
                join sc in reader.SolutionComponents on pa.Id equals sc.ObjectId
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
