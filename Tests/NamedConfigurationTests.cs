using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using XrmSync.Options;

namespace Tests;

public class NamedConfigurationTests
{
    [Fact]
    public void ResolveConfigurationName_WithSpecificName_ReturnsRequestedName()
    {
        // Arrange
        var configJson = """
        {
          "XrmSync": {
            "default": {
              "Plugin": {
                "Sync": {
                  "AssemblyPath": "default.dll"
                }
              }
            },
            "dev": {
              "Plugin": {
                "Sync": {
                  "AssemblyPath": "dev.dll"
                }
              }
            }
          }
        }
        """;
        
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, configJson);
        
        try
        {
            var configReader = new TestConfigReader(tempFile);
            
            // Act
            var result = configReader.ResolveConfigurationName("dev");
            
            // Assert
            Assert.Equal("dev", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ResolveConfigurationName_WithNoSpecificName_ReturnsDefault()
    {
        // Arrange
        var configJson = """
        {
          "XrmSync": {
            "default": {
              "Plugin": {
                "Sync": {
                  "AssemblyPath": "default.dll"
                }
              }
            },
            "dev": {
              "Plugin": {
                "Sync": {
                  "AssemblyPath": "dev.dll"
                }
              }
            }
          }
        }
        """;
        
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, configJson);
        
        try
        {
            var configReader = new TestConfigReader(tempFile);
            
            // Act
            var result = configReader.ResolveConfigurationName(null);
            
            // Assert
            Assert.Equal("default", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ResolveConfigurationName_WithSingleConfig_ReturnsThatConfig()
    {
        // Arrange
        var configJson = """
        {
          "XrmSync": {
            "myconfig": {
              "Plugin": {
                "Sync": {
                  "AssemblyPath": "myconfig.dll"
                }
              }
            }
          }
        }
        """;
        
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, configJson);
        
        try
        {
            var configReader = new TestConfigReader(tempFile);
            
            // Act
            var result = configReader.ResolveConfigurationName(null);
            
            // Assert
            Assert.Equal("myconfig", result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ResolveConfigurationName_WithLegacyStructure_ReturnsNull()
    {
        // Arrange
        var configJson = """
        {
          "XrmSync": {
            "Plugin": {
              "Sync": {
                "AssemblyPath": "legacy.dll"
              }
            }
          }
        }
        """;
        
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, configJson);
        
        try
        {
            var configReader = new TestConfigReader(tempFile);
            
            // Act
            var result = configReader.ResolveConfigurationName(null);
            
            // Assert
            Assert.Null(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ConfigurationBuilder_WithConfigName_LoadsCorrectConfiguration()
    {
        // Arrange
        var configJson = """
        {
          "XrmSync": {
            "dev": {
              "Plugin": {
                "Sync": {
                  "AssemblyPath": "dev.dll",
                  "SolutionName": "DevSolution",
                  "LogLevel": "Debug",
                  "DryRun": true
                }
              }
            }
          }
        }
        """;
        
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, configJson);
        
        try
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(tempFile)
                .Build();
            
            var builder = new XrmSyncConfigurationBuilder(configuration, "dev");
            
            // Act
            var result = builder.Build();
            
            // Assert
            Assert.NotNull(result.Plugin?.Sync);
            Assert.Equal("dev.dll", result.Plugin.Sync.AssemblyPath);
            Assert.Equal("DevSolution", result.Plugin.Sync.SolutionName);
            Assert.Equal(LogLevel.Debug, result.Plugin.Sync.LogLevel);
            Assert.True(result.Plugin.Sync.DryRun);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ConfigurationBuilder_WithoutConfigName_LoadsLegacyStructure()
    {
        // Arrange
        var configJson = """
        {
          "XrmSync": {
            "Plugin": {
              "Sync": {
                "AssemblyPath": "legacy.dll",
                "SolutionName": "LegacySolution",
                "LogLevel": "Information",
                "DryRun": false
              }
            }
          }
        }
        """;
        
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, configJson);
        
        try
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(tempFile)
                .Build();
            
            var builder = new XrmSyncConfigurationBuilder(configuration, null);
            
            // Act
            var result = builder.Build();
            
            // Assert
            Assert.NotNull(result.Plugin?.Sync);
            Assert.Equal("legacy.dll", result.Plugin.Sync.AssemblyPath);
            Assert.Equal("LegacySolution", result.Plugin.Sync.SolutionName);
            Assert.Equal(LogLevel.Information, result.Plugin.Sync.LogLevel);
            Assert.False(result.Plugin.Sync.DryRun);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private class TestConfigReader : IConfigReader
    {
        private readonly string _configFile;

        public TestConfigReader(string configFile)
        {
            _configFile = configFile;
        }

        public IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile(_configFile)
                .Build();
        }

        public string? ResolveConfigurationName(string? requestedName)
        {
            var configuration = GetConfiguration();
            var xrmSyncSection = configuration.GetSection("XrmSync");
            
            if (!xrmSyncSection.Exists())
            {
                return null;
            }

            // Get all configuration names (direct children of XrmSync)
            var configNames = xrmSyncSection.GetChildren()
                .Select(c => c.Key)
                .Where(k => k != "Plugin") // Exclude legacy structure
                .ToList();

            // If requested name is specified, use it if it exists
            if (!string.IsNullOrWhiteSpace(requestedName))
            {
                return configNames.Contains(requestedName) ? requestedName : null;
            }

            // If only one named config exists, use it
            if (configNames.Count == 1)
            {
                return configNames[0];
            }

            // If multiple configs exist, try to use "default"
            if (configNames.Contains("default"))
            {
                return "default";
            }

            // Fall back to legacy structure if no named configs exist
            if (configNames.Count == 0 && xrmSyncSection.GetSection("Plugin").Exists())
            {
                return null; // Use legacy structure
            }

            return null;
        }
    }
}
