using System.Reflection;
using System.Runtime.CompilerServices;

namespace XrmSync.Analyzer.Reader;

/// <summary>
/// Runs an analysis callback against an assembly loaded into a collectible
/// <see cref="PluginLoadContext"/>, then unloads it and reports whether the unload actually
/// completed.
///
/// Unloading is cooperative: the context is only collected once nothing references anything
/// inside it. Analysis instantiates plugin types and invokes their registration methods, so a
/// static field, a subscribed event or a background thread created by that code can keep the
/// context alive. That leaks memory but does not affect the analysis result, hence the caller
/// reports it as a warning rather than failing the sync.
/// </summary>
internal static class IsolatedAssemblyLoader
{
	internal record IsolatedResult<T>(T Value, bool Unloaded);

	/// <summary>
	/// The number of collect/finalize rounds to run before declaring the context leaked.
	/// Finalizers can resurrect references, so a single pass is not conclusive.
	/// </summary>
	private const int UnloadAttempts = 10;

	public static IsolatedResult<T> Run<T>(string assemblyPath, byte[] assemblyBytes, Func<Assembly, T> analyze)
	{
		var (value, contextRef) = RunIsolated(assemblyPath, assemblyBytes, analyze);
		return new IsolatedResult<T>(value, WaitForUnload(contextRef));
	}

	/// <summary>
	/// Kept in its own non-inlined frame so that no local still holds the load context or the
	/// loaded assembly by the time <see cref="WaitForUnload"/> runs.
	/// </summary>
	[MethodImpl(MethodImplOptions.NoInlining)]
	private static (T value, WeakReference contextRef) RunIsolated<T>(string assemblyPath, byte[] assemblyBytes, Func<Assembly, T> analyze)
	{
		var context = new PluginLoadContext(assemblyPath);
		try
		{
			var assembly = context.LoadMainAssembly(assemblyBytes);
			return (analyze(assembly), new WeakReference(context));
		}
		finally
		{
			context.Unload();
		}
	}

	[MethodImpl(MethodImplOptions.NoInlining)]
	private static bool WaitForUnload(WeakReference contextRef)
	{
		for (var attempt = 0; attempt < UnloadAttempts && contextRef.IsAlive; attempt++)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		return !contextRef.IsAlive;
	}
}
