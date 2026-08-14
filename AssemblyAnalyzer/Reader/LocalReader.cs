using Microsoft.Extensions.Logging;
using XrmSync.Analyzer;
using XrmSync.Model;
using XrmSync.Model.Webresource;

namespace XrmSync.Analyzer.Reader;

internal class LocalReader(ILogger<LocalReader> logger, IAssemblyAnalyzer analyzer) : ILocalReader
{
	private readonly Dictionary<string, AssemblyInfo> assemblyCache = [];

	/// <summary>
	/// Reads assembly information by analyzing the assembly in a collectible load context,
	/// which is unloaded again once the analysis completes.
	/// </summary>
	public async Task<AssemblyInfo> ReadAssemblyAsync(string assemblyDllPath, string publisherPrefix, CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(assemblyDllPath))
		{
			throw new AnalysisException("Assembly DLL path cannot be null or empty");
		}

		if (string.IsNullOrWhiteSpace(publisherPrefix))
		{
			throw new AnalysisException("Publisher prefix cannot be null or empty");
		}

		if (assemblyCache.TryGetValue(assemblyDllPath, out var cachedAssemblyInfo))
		{
			logger.LogTrace("Returning cached assembly info for {AssemblyName}", cachedAssemblyInfo.Name);
			return cachedAssemblyInfo;
		}

		logger.LogDebug("Reading assembly from {AssemblyDllPath}", assemblyDllPath);

		// Analysis runs arbitrary code from the analyzed assembly - plugin constructors and
		// registration methods - which cannot be interrupted. Passing the token to Task.Run
		// would therefore only cover the window before the delegate starts. WaitAsync is what
		// makes cancellation observable: the caller stops waiting immediately, and an abandoned
		// analysis is deliberately left to run to completion on the thread pool. It owns its
		// load context, so that context is still unloaded when it finishes.
		var analysis = Task.Run(() => analyzer.AnalyzeAssembly(assemblyDllPath, publisherPrefix), CancellationToken.None);
		var assemblyInfo = await analysis.WaitAsync(cancellationToken);

		logger.LogInformation("Local assembly read successfully: {AssemblyName} version {Version}", assemblyInfo.Name, assemblyInfo.Version);

		// Cache the assembly info
		assemblyCache[assemblyDllPath] = assemblyInfo;

		return assemblyInfo;
	}

	public List<WebresourceDefinition> ReadWebResourceFolder(string folderPath, string prefix, IEnumerable<string>? fileExtensions = null)
	{
		var absolutePath = Path.GetFullPath(folderPath);
		logger.LogInformation("Reading webresources from folder: {FolderPath}", absolutePath);
		if (!Directory.Exists(absolutePath))
		{
			throw new AnalysisException($"Webresource folder does not exist: {absolutePath}");
		}

		var allowedTypes = WebresourceTypeMap.ResolveTypes(fileExtensions);

		var files = Directory.EnumerateFiles(absolutePath, "*.*", SearchOption.AllDirectories);
		return [.. files.Select(f =>
			{
				var relativePath = Path.Combine(prefix, Path.GetRelativePath(absolutePath, f));
				var ext = Path.GetExtension(f).ToLowerInvariant();

				return (
					relativePath,
					fullPath: f,
					extension: ext
				);
			})
			.Where(f => WebresourceTypeMap.ExtensionToType.ContainsKey(f.extension) && (allowedTypes.Count == 0 || allowedTypes.Contains(WebresourceTypeMap.ExtensionToType[f.extension])))
			.Select(f => new WebresourceDefinition(
				Name: f.relativePath.Replace('\\', '/'),
				DisplayName: Path.GetFileName(f.relativePath),
				Type: WebresourceTypeMap.ExtensionToType[f.extension],
				Content: Convert.ToBase64String(File.ReadAllBytes(f.fullPath))
			))
			.OrderBy(d => d.Name)
		];
	}
}
