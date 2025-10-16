using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using XrmSync.Commands;
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
            var builder = new XrmSyncConfigurationBuilder(configReader.GetConfiguration(), Options.Create(SharedOptions.Empty with { ConfigName = "dev" }));

            // Act
            var configuration = builder.Build();
            
            // Assert
            Assert.Equal("dev.dll", configuration.Plugin.Sync.AssemblyPath);
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
            var builder = new XrmSyncConfigurationBuilder(configReader.GetConfiguration(), Options.Create(SharedOptions.Empty));

            // Act
            var result = builder.Build();

            // Assert
            Assert.Equal("default.dll", result.Plugin.Sync.AssemblyPath);
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
            var builder = new XrmSyncConfigurationBuilder(configReader.GetConfiguration(), Options.Create(SharedOptions.Empty));

            // Act
            var result = builder.Build();

            // Assert
            Assert.Equal("myconfig.dll", result.Plugin.Sync.AssemblyPath);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void ResolveConfigurationName_WithLegacyStructure_ReturnsNonNamed()
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
            var builder = new XrmSyncConfigurationBuilder(configReader.GetConfiguration(), Options.Create(SharedOptions.Empty));

            // Act
            var result = builder.Build();

            // Assert
            Assert.Equal("legacy.dll", result.Plugin.Sync.AssemblyPath);
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

            var options = SharedOptions.Empty with { ConfigName = "dev" };

            var builder = new XrmSyncConfigurationBuilder(configuration, Options.Create(options));
            
            // Act
            var result = builder.Build();
            
            // Assert
            Assert.NotNull(result.Plugin?.Sync);
            Assert.Equal("dev.dll", result.Plugin.Sync.AssemblyPath);
            Assert.Equal("DevSolution", result.Plugin.Sync.SolutionName);
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

            var builder = new XrmSyncConfigurationBuilder(configuration, Options.Create(SharedOptions.Empty));
            
            // Act
            var result = builder.Build();
            
            // Assert
            Assert.NotNull(result.Plugin?.Sync);
            Assert.Equal("legacy.dll", result.Plugin.Sync.AssemblyPath);
            Assert.Equal("LegacySolution", result.Plugin.Sync.SolutionName);
            Assert.False(result.Plugin.Sync.DryRun);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private class TestConfigReader(string configFile) : IConfigReader
    {
        public IConfiguration GetConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile(configFile)
                .Build();
        }
    }
}
