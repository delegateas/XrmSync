using DG.XrmSync.Model;

namespace DG.XrmSync.SyncService.AssemblyReader;

public interface IAssemblyReader
{
    Task<AssemblyInfo> ReadAssemblyAsync(string assemblyDllPath);
}
