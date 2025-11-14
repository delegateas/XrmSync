using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Text.Json;
using XrmSync.Model;
using XrmSync.Options;

namespace Tests.Config;

public class ConfigWriterTests
{
    private readonly ILogger<ConfigWriter> _logger;

    public ConfigWriterTests()
    {
        _logger = Substitute.For<ILogger<ConfigWriter>>();
    }

    [Fact]
    public async Task SaveConfig_CreatesNewFile_WithProfiles()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.Delete(tempFile); // Ensure file doesn't exist

        XrmSyncConfiguration config = new (
            DryRun: true,
            LogLevel: LogLevel.Debug,
            CiMode: false,
            Profiles: new List<ProfileConfiguration>
            {
                new("default", "TestSolution", new List<SyncItem>
                {
                    new PluginSyncItem("test.dll"),
                    new PluginAnalysisSyncItem("analysis.dll", "tst", true),
                    new WebresourceSyncItem("wwwroot")
                })
            }
        );

        var options = Options.Create(config);
        var configWriter = new ConfigWriter(options, _logger);

        try
        {
            // Act
            await configWriter.SaveConfig(tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));

            var content = await File.ReadAllTextAsync(tempFile);
            var json = JsonSerializer.Deserialize<JsonElement>(content);

            var xrmSyncSection = json.GetProperty("XrmSync");
            Assert.True(xrmSyncSection.GetProperty("DryRun").GetBoolean());
            Assert.Equal("Debug", xrmSyncSection.GetProperty("LogLevel").GetString());
            Assert.False(xrmSyncSection.GetProperty("CiMode").GetBoolean());

            var profiles = xrmSyncSection.GetProperty("Profiles");
            Assert.Equal(1, profiles.GetArrayLength());

            var profile = profiles[0];
            Assert.Equal("default", profile.GetProperty("Name").GetString());
            Assert.Equal("TestSolution", profile.GetProperty("SolutionName").GetString());

            var syncItems = profile.GetProperty("Sync");
            Assert.Equal(3, syncItems.GetArrayLength());

            var pluginSync = syncItems[0];
            Assert.Equal("Plugin", pluginSync.GetProperty("Type").GetString());
            Assert.Equal("test.dll", pluginSync.GetProperty("AssemblyPath").GetString());

            var pluginAnalysis = syncItems[1];
            Assert.Equal("PluginAnalysis", pluginAnalysis.GetProperty("Type").GetString());
            Assert.Equal("analysis.dll", pluginAnalysis.GetProperty("AssemblyPath").GetString());
            Assert.Equal("tst", pluginAnalysis.GetProperty("PublisherPrefix").GetString());
            Assert.True(pluginAnalysis.GetProperty("PrettyPrint").GetBoolean());

            var webresourceSync = syncItems[2];
            Assert.Equal("Webresource", webresourceSync.GetProperty("Type").GetString());
            Assert.Equal("wwwroot", webresourceSync.GetProperty("FolderPath").GetString());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveConfig_CreatesNewFile_WithMultipleProfiles()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        File.Delete(tempFile); // Ensure file doesn't exist

        XrmSyncConfiguration config = new (
            DryRun: true,
            LogLevel: LogLevel.Debug,
            CiMode: false,
            Profiles: new List<ProfileConfiguration>
            {
                new("default", "DefaultSolution", new List<SyncItem>
                {
                    new PluginSyncItem("default.dll")
                }),
                new("dev", "DevSolution", new List<SyncItem>
                {
                    new PluginSyncItem("dev.dll"),
                    new PluginAnalysisSyncItem("dev-analysis.dll", "dev", false)
                })
            }
        );

        var options = Options.Create(config);
        var configWriter = new ConfigWriter(options, _logger);

        try
        {
            // Act
            await configWriter.SaveConfig(tempFile);

            // Assert
            Assert.True(File.Exists(tempFile));

            var content = await File.ReadAllTextAsync(tempFile);
            var json = JsonSerializer.Deserialize<JsonElement>(content);

            var xrmSyncSection = json.GetProperty("XrmSync");
            var profiles = xrmSyncSection.GetProperty("Profiles");
            Assert.Equal(2, profiles.GetArrayLength());

            var defaultProfile = profiles[0];
            Assert.Equal("default", defaultProfile.GetProperty("Name").GetString());
            Assert.Equal("DefaultSolution", defaultProfile.GetProperty("SolutionName").GetString());

            var devProfile = profiles[1];
            Assert.Equal("dev", devProfile.GetProperty("Name").GetString());
            Assert.Equal("DevSolution", devProfile.GetProperty("SolutionName").GetString());

            var devSyncItems = devProfile.GetProperty("Sync");
            Assert.Equal(2, devSyncItems.GetArrayLength());
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

        XrmSyncConfiguration config = new (
            DryRun: true,
            LogLevel: LogLevel.Debug,
            CiMode: false,
            Profiles: new List<ProfileConfiguration>
            {
                new("default", "TestSolution", new List<SyncItem>
                {
                    new PluginSyncItem("test.dll"),
                    new PluginAnalysisSyncItem("analysis.dll", "new", false),
                    new WebresourceSyncItem("wwwroot")
                })
            }
        );

        var options = Options.Create(config);
        var configWriter = new ConfigWriter(options, _logger);

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
            var profiles = xrmSyncSection.GetProperty("Profiles");
            Assert.Equal(1, profiles.GetArrayLength());

            var profile = profiles[0];
            Assert.Equal("default", profile.GetProperty("Name").GetString());
            Assert.Equal("TestSolution", profile.GetProperty("SolutionName").GetString());
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public async Task SaveConfig_UpdatesExistingConfig()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var existingContent = """
        {
          "XrmSync": {
            "DryRun": false,
            "LogLevel": "Information",
            "CiMode": false,
            "Profiles": [
              {
                "Name": "default",
                "SolutionName": "OldSolution",
                "Sync": [
                  {
                    "Type": "Plugin",
                    "AssemblyPath": "old.dll"
                  }
                ]
              }
            ]
          }
        }
        """;
        await File.WriteAllTextAsync(tempFile, existingContent);

        XrmSyncConfiguration config = new (
            DryRun: true,
            LogLevel: LogLevel.Debug,
            CiMode: false,
            Profiles: new List<ProfileConfiguration>
            {
                new("default", "NewSolution", new List<SyncItem>
                {
                    new PluginSyncItem("new.dll"),
                    new PluginAnalysisSyncItem("new-analysis.dll", "new", true)
                }),
                new("dev", "DevSolution", new List<SyncItem>
                {
                    new PluginSyncItem("dev.dll")
                })
            }
        );

        var options = Options.Create(config);
        var configWriter = new ConfigWriter(options, _logger);

        try
        {
            // Act
            await configWriter.SaveConfig(tempFile);

            // Assert
            var content = await File.ReadAllTextAsync(tempFile);
            var json = JsonSerializer.Deserialize<JsonElement>(content);

            var xrmSyncSection = json.GetProperty("XrmSync");

            // Check config was updated
            Assert.True(xrmSyncSection.GetProperty("DryRun").GetBoolean());
            Assert.Equal("Debug", xrmSyncSection.GetProperty("LogLevel").GetString());

            var profiles = xrmSyncSection.GetProperty("Profiles");
            Assert.Equal(2, profiles.GetArrayLength());

            var defaultProfile = profiles[0];
            Assert.Equal("default", defaultProfile.GetProperty("Name").GetString());
            Assert.Equal("NewSolution", defaultProfile.GetProperty("SolutionName").GetString());

            var defaultSync = defaultProfile.GetProperty("Sync");
            Assert.Equal(2, defaultSync.GetArrayLength());
            Assert.Equal("new.dll", defaultSync[0].GetProperty("AssemblyPath").GetString());

            var devProfile = profiles[1];
            Assert.Equal("dev", devProfile.GetProperty("Name").GetString());
            Assert.Equal("DevSolution", devProfile.GetProperty("SolutionName").GetString());
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

        XrmSyncConfiguration config = new (
            DryRun: false,
            LogLevel: LogLevel.Debug,
            CiMode: false,
            Profiles: new List<ProfileConfiguration>
            {
                new("default", "TestSolution", new List<SyncItem>
                {
                    new PluginSyncItem("test.dll"),
                    new PluginAnalysisSyncItem("analysis.dll", "new", false),
                    new WebresourceSyncItem("wwwroot")
                })
            }
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
            var profiles = xrmSyncSection.GetProperty("Profiles");
            var profile = profiles[0];
            var syncItems = profile.GetProperty("Sync");
            Assert.Equal("test.dll", syncItems[0].GetProperty("AssemblyPath").GetString());
            Assert.Equal("TestSolution", profile.GetProperty("SolutionName").GetString());
        }
        finally
        {
            if (File.Exists(defaultFile))
                File.Delete(defaultFile);
        }
    }
}
