using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Runtime.CompilerServices;

using XrmSync.Model;
using XrmSync.Analyzer.Extensions;
using XrmSync.Analyzer.Analyzers;
using XrmSync.Analyzer.Reader;
using XrmSync.Model.Plugin;
using XrmSync.Model.CustomApi;

[assembly: InternalsVisibleTo("Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace XrmSync.Analyzer;

internal class AssemblyAnalyzer(
	ILogger<AssemblyAnalyzer> logger,
	IEnumerable<IAnalyzer<PluginDefinition>> pluginAnalyzers,
	IEnumerable<IAnalyzer<CustomApiDefinition>> customApiAnalyzers) : IAssemblyAnalyzer
{
	public AssemblyInfo AnalyzeAssembly(string dllPath, string prefix)
	{
		var dllFullPath = Path.GetFullPath(dllPath);

		if (!File.Exists(dllFullPath))
			throw new AnalysisException($"Assembly not found at {dllFullPath}");
		if (!Path.GetExtension(dllFullPath).Equals(".dll", StringComparison.OrdinalIgnoreCase))
			throw new AnalysisException($"Invalid assembly file type: {Path.GetExtension(dllFullPath)}, expected DLL");

		var dllName = Path.GetFileNameWithoutExtension(dllFullPath);
		var bytes = File.ReadAllBytes(dllFullPath);
		var hash = bytes.Sha1Checksum();

		var result = IsolatedAssemblyLoader.Run(dllFullPath, bytes, assembly => Analyze(assembly, dllName, dllFullPath, hash, prefix));

		if (!result.Unloaded)
		{
			logger.LogWarning(
				"The load context for {AssemblyName} could not be unloaded. Something in the analyzed assembly - a static field, an event subscription or a background thread created while reading its registrations - still references it, so it stays in memory until XrmSync exits. The analysis itself is unaffected.",
				dllName);
		}

		return result.Value;
	}

	private AssemblyInfo Analyze(Assembly assembly, string dllName, string dllFullPath, string hash, string prefix)
	{
		var assemblyVersion = assembly.GetName()?.Version?.ToString() ?? throw new AnalysisException("Could not determine assembly version");

		var types = assembly.GetLoadableTypes();
		if (!types.Any())
			throw new AnalysisException("No types found in the assembly. Ensure the assembly contains valid plugin or custom API types.");

		try
		{
			return new AssemblyInfo(dllName)
			{
				Version = assemblyVersion,
				Hash = hash,
				DllPath = dllFullPath,
				Plugins = [.. pluginAnalyzers.SelectMany(a => a.AnalyzeTypes(types, prefix)).OrderBy(d => d.Name)],
				CustomApis = [.. customApiAnalyzers.SelectMany(a => a.AnalyzeTypes(types, prefix)).OrderBy(d => d.Name)],
			};
		}
		catch (AggregateException ex)
		{
			var messages = string.Join(Environment.NewLine, ex.InnerExceptions.Select(e => $"  - {e.Message}"));
			throw new AnalysisException($"Assembly analysis failed with {ex.InnerExceptions.Count} error(s):{Environment.NewLine}{messages}", ex);
		}
	}
}
