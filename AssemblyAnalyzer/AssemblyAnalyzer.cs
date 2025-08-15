using System.Reflection;
using System.Runtime.CompilerServices;

using XrmSync.Model;
using XrmSync.AssemblyAnalyzer.Extensions;
using XrmSync.AssemblyAnalyzer.Analyzers;

[assembly: InternalsVisibleTo("Tests")]
namespace XrmSync.AssemblyAnalyzer;

public class AssemblyAnalyzer(IEnumerable<IPluginAnalyzer> pluginAnalyzers, IEnumerable<ICustomApiAnalyzer> customApiAnalyzers) : IAssemblyAnalyzer
{
    public AssemblyInfo AnalyzeAssembly(string dllPath, string prefix)
    {
        var dllFullPath = Path.GetFullPath(dllPath);

        if (!File.Exists(dllFullPath))
            throw new AnalysisException($"Assembly not found at {dllFullPath}");
        if (!Path.GetExtension(dllFullPath).Equals(".dll", StringComparison.OrdinalIgnoreCase))
            throw new AnalysisException($"Invalid assembly file type: {Path.GetExtension(dllFullPath)}, expected DLL");

        var dllname = Path.GetFileNameWithoutExtension(dllFullPath);
        var hash = File.ReadAllBytes(dllFullPath).Sha1Checksum();

        var assembly = Assembly.LoadFrom(dllFullPath);
        var assemblyVersion = assembly.GetName()?.Version?.ToString() ?? throw new AnalysisException("Could not determine assembly version");

        var types = assembly.GetLoadableTypes();
        if (!types.Any())
            throw new AnalysisException("No types found in the assembly. Ensure the assembly contains valid plugin or custom API types.");

        return new AssemblyInfo
        {
            Name = dllname,
            Version = assemblyVersion,
            Hash = hash,
            DllPath = dllFullPath,
            Plugins = [.. pluginAnalyzers.SelectMany(a => a.GetPluginDefinitions(types, prefix)).OrderBy(d => d.Name)],
            CustomApis = [.. customApiAnalyzers.SelectMany(a => a.GetCustomApis(types, prefix)).OrderBy(d => d.Name)],
        };
    }
}
