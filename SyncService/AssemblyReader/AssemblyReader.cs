using DG.XrmPluginSync.Model;
using System.Diagnostics;
using System.Text.Json;

namespace DG.XrmPluginSync.SyncService.AssemblyReader;

internal class AssemblyReader : IAssemblyReader
{
    public async Task<PluginAssembly> ReadAssemblyAsync(string assemblyDllPath)
    {
        var analyzerExePath = Path.Combine(AppContext.BaseDirectory, "AssemblyAnalyzer.exe");
        var analyzerWorkingDir = AppContext.BaseDirectory;

        var psi = new ProcessStartInfo
        {
            FileName = analyzerExePath,
            WorkingDirectory = analyzerWorkingDir,
            Arguments = assemblyDllPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start process.");

        // Read output and error asynchronously
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();

        // Wait for process to exit and for output reading to complete
        await process.WaitForExitAsync();
        var output = await outputTask;
        var error = await errorTask;

        // Optionally, handle errors
        if (process.ExitCode != 0)
        {
            // Log or throw with error
            throw new Exception($"Process failed: {error}");
        }

        // Process the output
        var assemblyInfo = JsonSerializer.Deserialize<PluginAssembly>(output);
        return assemblyInfo ?? throw new Exception("Failed to read plugin type information from assembly");
    }
}
