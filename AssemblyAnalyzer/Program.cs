using DG.XrmPluginSync.AssemblyAnalyzer;
using System.Text.Json;

var assemblyLocation = Path.GetFullPath(args[0]);
if (!File.Exists(assemblyLocation))
{
    throw new FileNotFoundException($"Assembly not found at {assemblyLocation}");
}

if (!Path.GetExtension(assemblyLocation).Equals(".dll", StringComparison.OrdinalIgnoreCase))
{
    throw new InvalidOperationException($"Invalid assembly file type: {Path.GetExtension(assemblyLocation)}, expected DLL");
}

var pluginDto = AssemblyAnalyzer.GetPluginAssembly(assemblyLocation);
var jsonOutput = JsonSerializer.Serialize(pluginDto);
Console.WriteLine(jsonOutput);
