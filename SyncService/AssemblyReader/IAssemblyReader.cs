using DG.XrmPluginSync.Model;

namespace DG.XrmPluginSync.SyncService.AssemblyReader;

public interface IAssemblyReader
{
    Task<PluginAssembly> ReadAssemblyAsync(string assemblyDllPath);
}
