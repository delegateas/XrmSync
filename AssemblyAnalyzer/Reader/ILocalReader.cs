using XrmSync.Model;
using XrmSync.Model.Webresource;

namespace XrmSync.Analyzer.Reader;

public interface ILocalReader
{
	/// <summary>
	/// Reads assembly information by executing XrmSync analyze command in a separate process.
	/// This class handles three different execution scenarios:
	/// 1. Debug mode: Uses the current process executable
	/// 2. Local dotnet tool: Uses "dotnet tool run xrmsync"
	/// 3. Global dotnet tool: Uses "xrmsync" directly
	/// The class automatically detects the appropriate method based on tool availability.
	/// </summary>
	Task<AssemblyInfo> ReadAssemblyAsync(string assemblyDllPath, string publisherPrefix, CancellationToken cancellationToken);

	List<WebresourceDefinition> ReadWebResourceFolder(string folderPath, string prefix);
}
