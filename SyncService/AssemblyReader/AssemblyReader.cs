using System.Reflection;
using System.Runtime.Loader;

namespace DG.XrmPluginSync.SyncService.AssemblyReader;

internal class AssemblyReader() : IAssemblyReader
{
    private bool disposedValue;

    private TestAssemblyLoadContext? Context { get; set; }

    public Assembly ReadAssembly(string assemblyDllPath)
    {
        if (Context == null)
        {
            Context = new TestAssemblyLoadContext(assemblyDllPath);
        }

        return Context.LoadFromAssemblyPath(assemblyDllPath);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing && Context != null)
            {
                Context.Unload();
                Context = null;
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    class TestAssemblyLoadContext(string mainAssemblyToLoadPath) : AssemblyLoadContext("PluginAssembly", isCollectible: true)
    {
        private AssemblyDependencyResolver _resolver = new AssemblyDependencyResolver(mainAssemblyToLoadPath);

        protected override Assembly? Load(AssemblyName name)
        {
            string? assemblyPath = _resolver.ResolveAssemblyToPath(name);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
    }
}
