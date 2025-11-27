using XrmSync.Model;

namespace XrmSync.Dataverse.Interfaces;

public interface IPluginAssemblyReader
{
	AssemblyInfo? GetPluginAssembly(Guid solutionId, string assemblyName);
}
