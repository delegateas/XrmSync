using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text.Json;
using XrmSync.Model;
using XrmSync.Options;

namespace Tests;

public class ConfigWriterTests
{
    private readonly ILogger<ConfigWriter> _logger;
    private readonly ConfigWriter _configWriter;

    public ConfigWriterTests()
    {
        _logger = Substitute.For<ILogger<ConfigWriter>>();
        _configWriter = new ConfigWriter(_logger);
    }

    [Fact]
    public async Task SaveConfigAsync_CreatesNewFile_WhenFileDoesNotExist()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.Delete(tempFile); // Ensure file doesn't exist
        
        var options = new PluginSyncOptions(
            AssemblyPath: "test.dll",
            SolutionName: "TestSolution",
            LogLevel: LogLevel.Debug,
            DryRun: true
        );

        try
        {
            // Act
            await _configWriter.SavePluginSyncConfigAsync(options, tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));
            
            var content = await File.ReadAllTextAsync(tempFile);
            var config = JsonSerializer.Deserialize<JsonElement>(content);
            
            var xrmSyncSection = config.GetProperty("XrmSync");
            var pluginSection = xrmSyncSection.GetProperty("Plugin");
            var syncSection = pluginSection.GetProperty("Sync");
            Assert.Equal("test.dll", syncSection.GetProperty("AssemblyPath").GetString());
            Assert.Equal("TestSolution", syncSection.GetProperty("SolutionName").GetString());
            Assert.Equal("Debug", syncSection.GetProperty("LogLevel").GetString());
            Assert.True(syncSection.GetProperty("DryRun").GetBoolean());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveConfigAsync_MergesWithExistingFile_WhenFileExists()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var existingContent = """
        {
          "SomeOtherSection": {
            "SomeProperty": "SomeValue"
          }
        }
        """;
        await File.WriteAllTextAsync(tempFile, existingContent);
        
        var options = new PluginSyncOptions(
            AssemblyPath: "test.dll",
            SolutionName: "TestSolution",
            LogLevel: LogLevel.Information,
            DryRun: false
        );

        try
        {
            // Act
            await _configWriter.SavePluginSyncConfigAsync(options, tempFile);

            // Assert
            var content = await File.ReadAllTextAsync(tempFile);
            var config = JsonSerializer.Deserialize<JsonElement>(content);
            
            // Check original section is preserved
            var otherSection = config.GetProperty("SomeOtherSection");
            Assert.Equal("SomeValue", otherSection.GetProperty("SomeProperty").GetString());
            
            // Check XrmSync section is added
            var xrmSyncSection = config.GetProperty("XrmSync");
            var pluginSection = xrmSyncSection.GetProperty("Plugin");
            var syncSection = pluginSection.GetProperty("Sync");
            Assert.Equal("test.dll", syncSection.GetProperty("AssemblyPath").GetString());
            Assert.Equal("TestSolution", syncSection.GetProperty("SolutionName").GetString());
            Assert.Equal("Information", syncSection.GetProperty("LogLevel").GetString());
            Assert.False(syncSection.GetProperty("DryRun").GetBoolean());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveConfigAsync_UsesDefaultFileName_WhenFilePathIsNull()
    {
        // Arrange
        var currentDir = Directory.GetCurrentDirectory();
        var defaultFile = Path.Combine(currentDir, "appsettings.json");
        
        // Clean up if file exists
        if (File.Exists(defaultFile))
            File.Delete(defaultFile);
        
        var options = new PluginSyncOptions(
            AssemblyPath: "test.dll",
            SolutionName: "TestSolution",
            LogLevel: LogLevel.Warning,
            DryRun: false
        );

        try
        {
            // Act
            await _configWriter.SavePluginSyncConfigAsync(options);

            // Assert
            Assert.True(File.Exists(defaultFile));
            
            var content = await File.ReadAllTextAsync(defaultFile);
            var config = JsonSerializer.Deserialize<JsonElement>(content);
            
            var xrmSyncSection = config.GetProperty("XrmSync");
            var pluginSection = xrmSyncSection.GetProperty("Plugin");
            var syncSection = pluginSection.GetProperty("Sync");
            Assert.Equal("test.dll", syncSection.GetProperty("AssemblyPath").GetString());
            Assert.Equal("TestSolution", syncSection.GetProperty("SolutionName").GetString());
            Assert.Equal("Warning", syncSection.GetProperty("LogLevel").GetString());
            Assert.False(syncSection.GetProperty("DryRun").GetBoolean());
        }
        finally
        {
            if (File.Exists(defaultFile))
                File.Delete(defaultFile);
        }
    }
}