using System.Reflection;
using System.Runtime.Loader;

namespace XrmSync.Analyzer.Reader;

/// <summary>
/// Collectible load context used to analyze a single plugin assembly without permanently
/// loading it into the default context.
///
/// Resolution order mirrors what <see cref="Assembly.LoadFrom(string)"/> did in the default
/// context, so the analyzers keep working across the context boundary:
/// 1. Anything the host can already resolve (XrmPluginCore.Abstractions, framework facades)
///    is served from the default context, so type identity is shared where it matters.
/// 2. Everything else is probed for next to the analyzed assembly.
///
/// Assemblies are always loaded from a byte array, never from a path, so no file handle is
/// held on the DLL - watch mode rebuilds the assembly while XrmSync is running.
/// </summary>
internal sealed class PluginLoadContext(string assemblyPath)
	: AssemblyLoadContext($"XrmSync:{Path.GetFileNameWithoutExtension(assemblyPath)}", isCollectible: true)
{
	private readonly string probeDirectory = Path.GetDirectoryName(Path.GetFullPath(assemblyPath)) ?? ".";

	/// <summary>
	/// Loads the assembly under analysis into this context.
	/// </summary>
	public Assembly LoadMainAssembly(byte[] assemblyBytes)
	{
		using var stream = new MemoryStream(assemblyBytes, writable: false);
		return LoadFromStream(stream);
	}

	protected override Assembly? Load(AssemblyName assemblyName)
	{
		if (assemblyName.Name is not { Length: > 0 } name)
			return null;

		// Prefer the host's copy of shared contracts. The analyzers compare against
		// typeof(IPluginDefinition)/typeof(ICustomApiDefinition) as a fallback, which only
		// works if those come from the same assembly the host is running.
		if (TryLoadFromDefault(assemblyName) is { } shared)
			return shared;

		var candidate = Path.Combine(probeDirectory, name + ".dll");
		if (!File.Exists(candidate))
			return null; // Let the runtime fall back to the default context

		return LoadMainAssembly(File.ReadAllBytes(candidate));
	}

	private static Assembly? TryLoadFromDefault(AssemblyName assemblyName)
	{
		try
		{
			return Default.LoadFromAssemblyName(assemblyName);
		}
		catch (FileNotFoundException)
		{
			return null;
		}
		catch (FileLoadException)
		{
			return null;
		}
		catch (BadImageFormatException)
		{
			return null;
		}
	}
}
