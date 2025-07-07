using DG.XrmSync.Model;
using DG.XrmSync.SyncService.AssemblyReader;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace DG.XrmSync.AssemblyAnalyzer.AssemblyReader;

internal class AssemblyReader(ILogger logger) : IAssemblyReader
{
    private Dictionary<string, AssemblyInfo> assemblyCache = new();

    public async Task<AssemblyInfo> ReadAssemblyAsync(string assemblyDllPath)
    {
        if (string.IsNullOrWhiteSpace(assemblyDllPath))
        {
            throw new ArgumentException("Assembly DLL path cannot be null or empty.", nameof(assemblyDllPath));
        }

        if (assemblyCache.TryGetValue(assemblyDllPath, out var cachedAssemblyInfo))
        {
            logger.LogTrace("Returning cached assembly info for {AssemblyName}", cachedAssemblyInfo.Name);
            return cachedAssemblyInfo;
        }

        logger.LogDebug("Reading assembly from {AssemblyDllPath}", assemblyDllPath);
        var assemblyInfo = await ReadAssemblyInternalAsync(assemblyDllPath);
        
        // Cache the assembly info
        assemblyCache[assemblyDllPath] = assemblyInfo;
        
        return assemblyInfo;
    }

    private async Task<AssemblyInfo> ReadAssemblyInternalAsync(string assemblyDllPath)
    {
        var args = $"analyze --assembly \"{assemblyDllPath}\"";
#if DEBUG
        // In debug, invoke the currently executing assembly
        var filename = Process.GetCurrentProcess().MainModule?.FileName ?? "";
#else
        // In release, invoke as a dotnet tool
        const string filename = "dotnet";
        args = $"tool run XrmSync {args}";
#endif
        var psi = new ProcessStartInfo
        {
            FileName = filename,
            Arguments = args,
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
        var assemblyInfo = JsonSerializer.Deserialize<AssemblyInfo>(output);

        logger.LogInformation("Local assembly read successfully: {AssemblyName} version {Version}", assemblyInfo?.Name, assemblyInfo?.Version);

        return assemblyInfo ?? throw new Exception("Failed to read plugin type information from assembly");
    }
}
