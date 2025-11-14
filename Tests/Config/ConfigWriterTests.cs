using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Text.Json;
using XrmSync.Model;
using XrmSync.Options;

namespace Tests.Config;

public class ConfigWriterTests
{
    private readonly ILogger<ConfigWriter> logger;

    public ConfigWriterTests()
    {
        logger = Substitute.For<ILogger<ConfigWriter>>();
    }

    [Fact]
    public async Task SaveConfigCreatesNewFileUsesDefaultConfigName()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.Delete(tempFile); // Ensure file doesn't exist
        
        XrmSyncConfiguration config = new (
            new (
                new ("test.dll", "TestSolution"),
                new ("analysis.dll", "tst", true)
            ),
            new (
                new ("wwwroot", "WebSolution")
            ),
            new (LogLevel.Debug, false),
            new (true)
        );

        var options = Options.Create(config);
        var configWriter = new ConfigWriter(options, logger);

        try
        {
            // Act
            await configWriter.SaveConfig(tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));
            
            var content = await File.ReadAllTextAsync(tempFile);
            var json = JsonSerializer.Deserialize<JsonElement>(content);
            
            var xrmSyncSection = json.GetProperty("XrmSync");
            var defaultConfigSection = xrmSyncSection.GetProperty("default");

            var pluginSection = defaultConfigSection.GetProperty("Plugin");
            var syncSection = pluginSection.GetProperty("Sync");
            Assert.Equal("test.dll", syncSection.GetProperty("AssemblyPath").GetString());
            Assert.Equal("TestSolution", syncSection.GetProperty("SolutionName").GetString());

            var analysisSection = pluginSection.GetProperty("Analysis");
            Assert.Equal("analysis.dll", analysisSection.GetProperty("AssemblyPath").GetString());
            Assert.Equal("tst", analysisSection.GetProperty("PublisherPrefix").GetString());
            Assert.True(analysisSection.GetProperty("PrettyPrint").GetBoolean());

            var webSection = defaultConfigSection.GetProperty("Webresource");
            var webSyncSection = webSection.GetProperty("Sync");
            Assert.Equal("wwwroot", webSyncSection.GetProperty("FolderPath").GetString());
            Assert.Equal("WebSolution", webSyncSection.GetProperty("SolutionName").GetString());

            var loggerSection = defaultConfigSection.GetProperty("Logger");
            Assert.Equal("Debug", loggerSection.GetProperty("LogLevel").GetString());
            Assert.False(loggerSection.GetProperty("CiMode").GetBoolean());
            
            var executionSection = defaultConfigSection.GetProperty("Execution");
            Assert.True(executionSection.GetProperty("DryRun").GetBoolean());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveConfigCreatesNewFileWithNamedStructure()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.Delete(tempFile); // Ensure file doesn't exist

        XrmSyncConfiguration config = new (
            new PluginOptions(
                new ("dev.dll", "DevSolution"),
                new ("dev-analysis.dll", "dev", false)
            ),
            new WebresourceOptions(
                new ("dev-wwwroot", "DevWebSolution")
            ),
            new (LogLevel.Debug, false),
            new (true)
        );

        var options = Options.Create(config);
        var configWriter = new ConfigWriter(options, logger);

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
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveConfigMergesWithExistingFilePreservingOtherSections()
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

        XrmSyncConfiguration config = new (
            new (
                new ("test.dll", "TestSolution"),
                new ("analysis.dll", "new", false)
            ),
            new (
                new ("wwwroot", "WebSolution")
            ),
            new (LogLevel.Debug, false),
            new (true)
        );

        var options = Options.Create(config);
        var configWriter = new ConfigWriter(options, logger);

        try
        {
            // Act
            await configWriter.SaveConfig(tempFile);

            // Assert
            var content = await File.ReadAllTextAsync(tempFile);
            var json = JsonSerializer.Deserialize<JsonElement>(content);
            
            // Check original section is preserved
            var otherSection = json.GetProperty("SomeOtherSection");
            Assert.Equal("SomeValue", otherSection.GetProperty("SomeProperty").GetString());
            
            // Check XrmSync section is added
            var xrmSyncSection = json.GetProperty("XrmSync");
            var defaultSection = xrmSyncSection.GetProperty("default");
            var pluginSection = defaultSection.GetProperty("Plugin");
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
    public async Task SaveConfigUpdatesExistingNamedConfigWithoutAffectingOtherConfigs()
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
                  "LogLevel": "Information"
                }
              },
              "Execution": {
                "DryRun": false
              }
            }
          }
        }
        """;
        await File.WriteAllTextAsync(tempFile, existingContent);

        XrmSyncConfiguration config = new (
            new (
                new ("dev.dll", "DevSolution"),
                new ("dev-analysis.dll", "dev", true)
            ),
            new (
                new ("dev-wwwroot", "DevWebSolution")
            ),
            new (LogLevel.Debug, false),
            new (true)
        );

        var options = Options.Create(config);
        var configWriter = new ConfigWriter(options, logger);

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
            var defaultExecution = defaultSection.GetProperty("Execution");
            Assert.False(defaultExecution.GetProperty("DryRun").GetBoolean());

            // Check dev config is added
            var devSection = xrmSyncSection.GetProperty("dev");
            var devSync = devSection.GetProperty("Plugin").GetProperty("Sync");
            Assert.Equal("dev.dll", devSync.GetProperty("AssemblyPath").GetString());
            Assert.Equal("DevSolution", devSync.GetProperty("SolutionName").GetString());
            var devExecution = devSection.GetProperty("Execution");
            Assert.True(devExecution.GetProperty("DryRun").GetBoolean());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveConfigUsesDefaultFileNameWhenFilePathIsNull()
    {
        // Arrange
        var currentDir = Directory.GetCurrentDirectory();
        var defaultFile = Path.Combine(currentDir, "appsettings.json");
        
        // Clean up if file exists
        if (File.Exists(defaultFile))
            File.Delete(defaultFile);

        XrmSyncConfiguration config = new (
            new (
                new ("test.dll", "TestSolution"),
                new ("analysis.dll", "new", false)
            ),
            new (
                new ("wwwroot", "WebSolution")
            ),
            new (LogLevel.Debug, false),
            new ExecutionOptions(false)
        );

        var options = Options.Create(config);
        var configWriter = new ConfigWriter(options, logger);

        try
        {
            // Act
            await configWriter.SaveConfig();

            // Assert
            Assert.True(File.Exists(defaultFile));
            
            var content = await File.ReadAllTextAsync(defaultFile);
            var json = JsonSerializer.Deserialize<JsonElement>(content);
            
            var xrmSyncSection = json.GetProperty("XrmSync");
            var defaultSection = xrmSyncSection.GetProperty("default");
            var pluginSection = defaultSection.GetProperty("Plugin");
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