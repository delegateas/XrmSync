using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using XrmSync.Model;
using XrmSync.Options;

namespace Tests.Config;

public class NamedConfigurationTests
{
    [Fact]
    public void ResolveConfigurationNameWithSpecificNameReturnsRequestedName()
    {
        // Arrange
        const string configJson = """
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
    public void ResolveConfigurationNameWithNoSpecificNameReturnsDefault()
    {
        // Arrange
        const string configJson = """
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
    public void ResolveConfigurationNameWithSingleConfigReturnsThatConfig()
    {
        // Arrange
        const string configJson = """
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
