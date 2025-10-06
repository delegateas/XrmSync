using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Exceptions;

namespace XrmSync.Dataverse;

public class PluginAssemblyWriter(IDataverseWriter writer, XrmSyncConfiguration configuration) : IPluginAssemblyWriter
{
    private Dictionary<string, object> Parameters { get; } = new() {
            { "SolutionUniqueName", configuration.Plugin?.Sync?.SolutionName ?? throw new XrmSyncException("No solution name found in configuration") }
    };

    public Guid CreatePluginAssembly(string pluginName, string dllPath, string sourceHash, string assemblyVersion, string description)
    {
        var entity = new PluginAssembly
        {
            Name = pluginName,
            Content = GetBase64StringFromFile(dllPath),
            SourceHash = sourceHash,
            IsolationMode = PluginAssembly_IsolationMode.Sandbox,
            Version = assemblyVersion,
            Description = description
        };

        return writer.Create(entity, Parameters);
    }

    public void UpdatePluginAssembly(Guid assemblyId, string pluginName, string dllPath, string sourceHash, string assemblyVersion, string description)
    {
        var entity = new PluginAssembly()
        {
            Id = assemblyId,
            Name = pluginName,
            Content = GetBase64StringFromFile(dllPath),
            SourceHash = sourceHash,
            IsolationMode = PluginAssembly_IsolationMode.Sandbox,
            Version = assemblyVersion,
            Description = description
        };

        writer.Update(entity);
    }

    private static string GetBase64StringFromFile(string dllPath)
    {
        // Reads the file at dllPath and returns its contents as a Base64 string
        if (string.IsNullOrWhiteSpace(dllPath))
            throw new XrmSyncException("DLL path must not be null or empty.");
        if (!File.Exists(dllPath))
            throw new XrmSyncException($"DLL file not found: {dllPath}");

        byte[] fileBytes = File.ReadAllBytes(dllPath);
        return Convert.ToBase64String(fileBytes);
    }
}
