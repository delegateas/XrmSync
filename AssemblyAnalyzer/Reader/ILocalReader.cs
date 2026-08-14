using XrmSync.Model;
using XrmSync.Model.Webresource;

namespace XrmSync.Analyzer.Reader;

public interface ILocalReader
{
	/// <summary>
	/// Reads assembly information by analyzing the assembly in a collectible load context,
	/// which is unloaded again once the analysis completes. Results are cached per DLL path.
	/// </summary>
	Task<AssemblyInfo> ReadAssemblyAsync(string assemblyDllPath, string publisherPrefix, CancellationToken cancellationToken);

	List<WebresourceDefinition> ReadWebResourceFolder(string folderPath, string prefix, IEnumerable<string>? fileExtensions = null);
}
