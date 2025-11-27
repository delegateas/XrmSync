namespace XrmSync.Dataverse.Interfaces;

public interface IPluginAssemblyWriter
{
	Guid CreatePluginAssembly(string pluginName, string dllPath, string sourceHash, string assemblyVersion, string description);
	void UpdatePluginAssembly(Guid assemblyId, string pluginName, string dllPath, string sourceHash, string assemblyVersion, string description);
}
