using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;

namespace XrmSync.Dataverse;

internal class PluginAssemblyReader(IDataverseReader reader) : IPluginAssemblyReader
{
    public AssemblyInfo? GetPluginAssembly(Guid solutionId, string assemblyName)
    {
        return (from pa in reader.PluginAssemblies
                join sc in reader.SolutionComponents on pa.Id equals sc.ObjectId
                where sc.SolutionId != null && sc.SolutionId.Id == solutionId && pa.Name == assemblyName
                select new AssemblyInfo(pa.Name ?? string.Empty)
                {
                    Id = pa.Id,
                    Version = pa.Version ?? string.Empty,
                    Hash = pa.SourceHash ?? string.Empty,
                }).FirstOrDefault();
    }
}
