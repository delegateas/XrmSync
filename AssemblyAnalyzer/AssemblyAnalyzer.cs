using System.Reflection;
using System.Runtime.CompilerServices;

using XrmSync.Model;
using XrmSync.Analyzer.Extensions;
using XrmSync.Analyzer.Analyzers;
using XrmSync.Model.Plugin;
using XrmSync.Model.CustomApi;

[assembly: InternalsVisibleTo("Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace XrmSync.Analyzer;

internal class AssemblyAnalyzer(IEnumerable<IAnalyzer<PluginDefinition>> pluginAnalyzers, IEnumerable<IAnalyzer<CustomApiDefinition>> customApiAnalyzers) : IAssemblyAnalyzer
{
	public AssemblyInfo AnalyzeAssembly(string dllPath, string prefix)
	{
		var dllFullPath = Path.GetFullPath(dllPath);

		if (!File.Exists(dllFullPath))
			throw new AnalysisException($"Assembly not found at {dllFullPath}");
		if (!Path.GetExtension(dllFullPath).Equals(".dll", StringComparison.OrdinalIgnoreCase))
			throw new AnalysisException($"Invalid assembly file type: {Path.GetExtension(dllFullPath)}, expected DLL");

		var dllName = Path.GetFileNameWithoutExtension(dllFullPath);
		var hash = File.ReadAllBytes(dllFullPath).Sha1Checksum();

		var assembly = Assembly.LoadFrom(dllFullPath);
		var assemblyVersion = assembly.GetName()?.Version?.ToString() ?? throw new AnalysisException("Could not determine assembly version");

		var types = assembly.GetLoadableTypes();
		if (!types.Any())
			throw new AnalysisException("No types found in the assembly. Ensure the assembly contains valid plugin or custom API types.");

		return new AssemblyInfo(dllName)
		{
			Version = assemblyVersion,
			Hash = hash,
			DllPath = dllFullPath,
			Plugins = [.. pluginAnalyzers.SelectMany(a => a.AnalyzeTypes(types, prefix)).OrderBy(d => d.Name)],
			CustomApis = [.. customApiAnalyzers.SelectMany(a => a.AnalyzeTypes(types, prefix)).OrderBy(d => d.Name)],
		};
	}
}
