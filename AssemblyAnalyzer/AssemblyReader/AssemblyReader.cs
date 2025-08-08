﻿using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;
using XrmSync.Model;

namespace XrmSync.AssemblyAnalyzer.AssemblyReader;

/// <summary>
/// Reads assembly information by executing XrmSync analyze command in a separate process.
/// This class handles three different execution scenarios:
/// 1. Debug mode: Uses the current process executable
/// 2. Local dotnet tool: Uses "dotnet tool run xrmsync"
/// 3. Global dotnet tool: Uses "xrmsync" directly
/// The class automatically detects the appropriate method based on tool availability.
/// </summary>
internal class AssemblyReader(ILogger logger) : IAssemblyReader
{
    private Dictionary<string, AssemblyInfo> assemblyCache = new();

    public async Task<AssemblyInfo> ReadAssemblyAsync(string assemblyDllPath, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(assemblyDllPath))
        {
            throw new AnalysisException("Assembly DLL path cannot be null or empty");
        }

        if (assemblyCache.TryGetValue(assemblyDllPath, out var cachedAssemblyInfo))
        {
            logger.LogTrace("Returning cached assembly info for {AssemblyName}", cachedAssemblyInfo.Name);
            return cachedAssemblyInfo;
        }

        logger.LogDebug("Reading assembly from {AssemblyDllPath}", assemblyDllPath);
        var assemblyInfo = await ReadAssemblyInternalAsync(assemblyDllPath, cancellationToken);

        // Cache the assembly info
        assemblyCache[assemblyDllPath] = assemblyInfo;

        return assemblyInfo;
    }

    private async Task<AssemblyInfo> ReadAssemblyInternalAsync(string assemblyDllPath, CancellationToken cancellationToken)
    {
        var (filename, args) = await GetExecutionInfoAsync(assemblyDllPath, cancellationToken);

        var result = await RunCommandAsync(filename, args, cancellationToken);
        if (result.ExitCode != 0)
        {
            logger.LogError("Failed to read assembly: {Error}", result.Error);
            throw new AnalysisException($"Failed to read assembly: {result.Error}");
        }

        // Process the output
        var assemblyInfo = JsonSerializer.Deserialize<AssemblyInfo>(result.Output);

        logger.LogInformation("Local assembly read successfully: {AssemblyName} version {Version}", assemblyInfo?.Name, assemblyInfo?.Version);

        return assemblyInfo ?? throw new AnalysisException("Failed to read plugin type information from assembly");
    }

    private async Task<(string filename, string args)> GetExecutionInfoAsync(string assemblyDllPath, CancellationToken cancellationToken)
    {
        var baseArgs = $"analyze --assembly \"{assemblyDllPath}\"";

#if DEBUG
        // In debug, try to invoke the currently executing assembly first
        var currentProcess = Process.GetCurrentProcess().MainModule?.FileName ?? "";
        if (!string.IsNullOrEmpty(currentProcess))
        {
            logger.LogTrace("Using current process executable for analysis: {Executable}", currentProcess);
            return (currentProcess, baseArgs);
        }
#endif

        // Try to find XrmSync installation in order of preference:
        // 1. Local dotnet tool
        // 2. Global dotnet tool
        var toolLocation = await FindXrmSyncToolAsync(cancellationToken);
        
        if (toolLocation.IsLocal)
        {
            logger.LogTrace("Using local dotnet tool for analysis");
            return ("dotnet", $"tool run xrmsync {baseArgs}");
        }
        else if (toolLocation.IsGlobal)
        {
            logger.LogTrace("Using global dotnet tool for analysis");
            return ("xrmsync", baseArgs);
        }
        else
        {
            // Fallback to the original behavior
            logger.LogTrace("Using fallback dotnet tool run for analysis");
            return ("dotnet", $"tool run xrmsync {baseArgs}");
        }
    }

    private async Task<ToolLocation> FindXrmSyncToolAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Check for local tool first
            var localResult = await RunCommandAsync("dotnet", "tool list", cancellationToken);
            if (localResult.ExitCode == 0 && localResult.Output.Contains("xrmsync"))
            {
                logger.LogDebug("Found XrmSync as local dotnet tool");
                return new ToolLocation { IsLocal = true };
            }

            // Check for global tool
            var globalResult = await RunCommandAsync("dotnet", "tool list -g", cancellationToken);
            if (globalResult.ExitCode == 0 && globalResult.Output.Contains("xrmsync"))
            {
                logger.LogDebug("Found XrmSync as global dotnet tool");
                return new ToolLocation { IsGlobal = true };
            }

            // Alternative check: try to run xrmsync directly to see if it's available globally
            var directResult = await RunCommandAsync("xrmsync", "--help", cancellationToken);
            if (directResult.ExitCode == 0)
            {
                logger.LogDebug("XrmSync executable found in PATH");
                return new ToolLocation { IsGlobal = true };
            }
        }
        catch (Exception ex)
        {
            logger.LogDebug("Error checking for dotnet tool installations: {Error}", ex.Message);
        }

        logger.LogDebug("XrmSync tool not found in local or global installations");
        return new ToolLocation();
    }

    private static async Task<CommandResult> RunCommandAsync(string fileName, string arguments, CancellationToken cancellationToken)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                return new CommandResult { ExitCode = -1, Output = "", Error = "Failed to start process" };
            }

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);
            var output = await outputTask;
            var error = await errorTask;

            return new CommandResult
            {
                ExitCode = process.ExitCode,
                Output = output,
                Error = error
            };
        }
        catch (Exception ex)
        {
            return new CommandResult { ExitCode = -1, Output = "", Error = ex.Message };
        }
    }

    private record ToolLocation
    {
        public bool IsLocal { get; init; }
        public bool IsGlobal { get; init; }
    }

    private record CommandResult
    {
        public required int ExitCode { get; init; }
        public required string Output { get; init; }
        public required string Error { get; init; }
    }
}
