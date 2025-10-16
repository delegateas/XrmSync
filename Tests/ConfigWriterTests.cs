using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Text.Json;
using XrmSync.Model;
using XrmSync.Options;

namespace Tests;

public class ConfigWriterTests
{
    private readonly ILogger<ConfigWriter> _logger;

    public ConfigWriterTests()
    {
        _logger = Substitute.For<ILogger<ConfigWriter>>();
    }

    [Fact]
    public async Task SaveConfig_CreatesNewFile_WithLegacyStructure()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.Delete(tempFile); // Ensure file doesn't exist
        
        var config = new XrmSyncConfiguration(
            new PluginOptions(
                new PluginSyncOptions("test.dll", "TestSolution", true),
                new PluginAnalysisOptions("analysis.dll", "tst", true)
            ),
            new WebresourceOptions(
                new WebresourceSyncOptions("wwwroot", "WebSolution", false)
            ),
            new LoggerOptions(LogLevel.Debug, false)
        );

        var options = Options.Create(config);
        var configWriter = new ConfigWriter(options, _logger);

        try
        {
            // Act
            await configWriter.SaveConfig(tempFile, configName: null);

            // Assert
            Assert.True(File.Exists(tempFile));
            
            var content = await File.ReadAllTextAsync(tempFile);
            var json = JsonSerializer.Deserialize<JsonElement>(content);
            
            var xrmSyncSection = json.GetProperty("XrmSync");
            var pluginSection = xrmSyncSection.GetProperty("Plugin");
            var syncSection = pluginSection.GetProperty("Sync");
            Assert.Equal("test.dll", syncSection.GetProperty("AssemblyPath").GetString());
            Assert.Equal("TestSolution", syncSection.GetProperty("SolutionName").GetString());
            Assert.True(syncSection.GetProperty("DryRun").GetBoolean());

            var analysisSection = pluginSection.GetProperty("Analysis");
            Assert.Equal("analysis.dll", analysisSection.GetProperty("AssemblyPath").GetString());
            Assert.Equal("tst", analysisSection.GetProperty("PublisherPrefix").GetString());
            Assert.True(analysisSection.GetProperty("PrettyPrint").GetBoolean());

            var webSection = xrmSyncSection.GetProperty("Webresource");
            var webSyncSection = webSection.GetProperty("Sync");
            Assert.Equal("wwwroot", webSyncSection.GetProperty("FolderPath").GetString());
            Assert.Equal("WebSolution", webSyncSection.GetProperty("SolutionName").GetString());

            var loggerSection = xrmSyncSection.GetProperty("Logger");
            Assert.Equal("Debug", loggerSection.GetProperty("LogLevel").GetString());
            Assert.False(loggerSection.GetProperty("CiMode").GetBoolean());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveConfig_CreatesNewFile_WithNamedStructure()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.Delete(tempFile); // Ensure file doesn't exist
        
        var config = new XrmSyncConfiguration(
            new PluginOptions(
                new PluginSyncOptions("dev.dll", "DevSolution", true),
                new PluginAnalysisOptions("dev-analysis.dll", "dev", false)
            ),
            new WebresourceOptions(
                new WebresourceSyncOptions("dev-wwwroot", "DevWebSolution", true)
            ),
            new LoggerOptions(LogLevel.Debug, false)
        );

        var options = Options.Create(config);
        var configWriter = new ConfigWriter(options, _logger);

        try
        {
            // Act
            await configWriter.SaveConfig(tempFile, configName: "dev");

            // Assert
            Assert.True(File.Exists(tempFile));
            
            var content = await File.ReadAllTextAsync(tempFile);
            var json = JsonSerializer.Deserialize<JsonElement>(content);
            
            var xrmSyncSection = json.GetProperty("XrmSync");
            var devSection = xrmSyncSection.GetProperty("dev");
            var pluginSection = devSection.GetProperty("Plugin");
            var syncSection = pluginSection.GetProperty("Sync");
            Assert.Equal("dev.dll", syncSection.GetProperty("AssemblyPath").GetString());
            Assert.Equal("DevSolution", syncSection.GetProperty("SolutionName").GetString());
            Assert.True(syncSection.GetProperty("DryRun").GetBoolean());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveConfig_MergesWithExistingFile_PreservingOtherSections()
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
        
        var config = new XrmSyncConfiguration(
            new PluginOptions(
                new PluginSyncOptions("test.dll", "TestSolution", false),
                new PluginAnalysisOptions("analysis.dll", "new", false)
            ),
            new WebresourceOptions(
                new WebresourceSyncOptions("wwwroot", "WebSolution", false)
            ),
            new LoggerOptions(LogLevel.Debug, false)
        );

        var options = Options.Create(config);
        var configWriter = new ConfigWriter(options, _logger);

        try
        {
            // Act
            await configWriter.SaveConfig(tempFile, configName: null);

            // Assert
            var content = await File.ReadAllTextAsync(tempFile);
            var json = JsonSerializer.Deserialize<JsonElement>(content);
            
            // Check original section is preserved
            var otherSection = json.GetProperty("SomeOtherSection");
            Assert.Equal("SomeValue", otherSection.GetProperty("SomeProperty").GetString());
            
            // Check XrmSync section is added
            var xrmSyncSection = json.GetProperty("XrmSync");
            var pluginSection = xrmSyncSection.GetProperty("Plugin");
            var syncSection = pluginSection.GetProperty("Sync");
            Assert.Equal("test.dll", syncSection.GetProperty("AssemblyPath").GetString());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveConfig_UpdatesExistingNamedConfig_WithoutAffectingOtherConfigs()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var existingContent = """
        {
          "XrmSync": {
            "default": {
              "Plugin": {
                "Sync": {
                  "AssemblyPath": "default.dll",
                  "SolutionName": "DefaultSolution",
                  "LogLevel": "Information",
                  "DryRun": false
                }
              }
            }
          }
        }
        """;
        await File.WriteAllTextAsync(tempFile, existingContent);
        
        var config = new XrmSyncConfiguration(
            new PluginOptions(
                new PluginSyncOptions("dev.dll", "DevSolution", true),
                new PluginAnalysisOptions("dev-analysis.dll", "dev", true)
            ),
            new WebresourceOptions(
                new WebresourceSyncOptions("dev-wwwroot", "DevWebSolution", true)
            ),
            new LoggerOptions(LogLevel.Debug, false)
        );

        var options = Options.Create(config);
        var configWriter = new ConfigWriter(options, _logger);

        try
        {
            // Act
            await configWriter.SaveConfig(tempFile, configName: "dev");

            // Assert
            var content = await File.ReadAllTextAsync(tempFile);
            var json = JsonSerializer.Deserialize<JsonElement>(content);
            
            var xrmSyncSection = json.GetProperty("XrmSync");
            
            // Check default config is preserved
            var defaultSection = xrmSyncSection.GetProperty("default");
            var defaultSync = defaultSection.GetProperty("Plugin").GetProperty("Sync");
            Assert.Equal("default.dll", defaultSync.GetProperty("AssemblyPath").GetString());
            
            // Check dev config is added
            var devSection = xrmSyncSection.GetProperty("dev");
            var devSync = devSection.GetProperty("Plugin").GetProperty("Sync");
            Assert.Equal("dev.dll", devSync.GetProperty("AssemblyPath").GetString());
            Assert.Equal("DevSolution", devSync.GetProperty("SolutionName").GetString());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveConfig_UsesDefaultFileName_WhenFilePathIsNull()
    {
        // Arrange
        var currentDir = Directory.GetCurrentDirectory();
        var defaultFile = Path.Combine(currentDir, "appsettings.json");
        
        // Clean up if file exists
        if (File.Exists(defaultFile))
            File.Delete(defaultFile);
        
        var config = new XrmSyncConfiguration(
            new PluginOptions(
                new PluginSyncOptions("test.dll", "TestSolution", false),
                new PluginAnalysisOptions("analysis.dll", "new", false)
            ),
            new WebresourceOptions(
                new WebresourceSyncOptions("wwwroot", "WebSolution", false)
            ),
            new LoggerOptions(LogLevel.Debug, false)
        );

        var options = Options.Create(config);
        var configWriter = new ConfigWriter(options, _logger);

        try
        {
            // Act
            await configWriter.SaveConfig();

            // Assert
            Assert.True(File.Exists(defaultFile));
            
            var content = await File.ReadAllTextAsync(defaultFile);
            var json = JsonSerializer.Deserialize<JsonElement>(content);
            
            var xrmSyncSection = json.GetProperty("XrmSync");
            var pluginSection = xrmSyncSection.GetProperty("Plugin");
            var syncSection = pluginSection.GetProperty("Sync");
            Assert.Equal("test.dll", syncSection.GetProperty("AssemblyPath").GetString());
            Assert.Equal("TestSolution", syncSection.GetProperty("SolutionName").GetString());
        }
        finally
        {
            if (File.Exists(defaultFile))
                File.Delete(defaultFile);
        }
    }
}