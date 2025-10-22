using System.Reflection;
using System.Security.Cryptography;

namespace XrmSync.Analyzer.Extensions;

internal static class AssemblyExtensions {
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

    public static string Sha1Checksum(this byte[] bytes)
    {
        return BitConverter.ToString(SHA1.HashData(bytes))
            .Replace("-", string.Empty);
    }
}
