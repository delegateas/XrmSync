using System.Reflection;

namespace DG.XrmPluginSync.AssemblyAnalyzer;

internal static class Extensions
{
    public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t != null).Select(t => t!);
        }
    }
}
