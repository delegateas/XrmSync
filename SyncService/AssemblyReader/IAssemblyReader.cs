using DG.XrmPluginSync.Model;
using System.Reflection;

namespace DG.XrmPluginSync.SyncService.AssemblyReader;

public interface IAssemblyReader : IDisposable
{
    Assembly ReadAssembly(string assemblyDllPath);
}
