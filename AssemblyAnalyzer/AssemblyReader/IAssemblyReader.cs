using XrmSync.Model;

namespace XrmSync.AssemblyAnalyzer.AssemblyReader;

public interface IAssemblyReader
{
    Task<AssemblyInfo> ReadAssemblyAsync(string assemblyDllPath, CancellationToken cancellationToken);
}
