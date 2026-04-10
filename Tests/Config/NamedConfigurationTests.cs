using Microsoft.Extensions.Configuration;
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
            "DryRun": false,
            "LogLevel": "Information",
            "CiMode": false,
            "Profiles": [
              {
                "Name": "default",
                "SolutionName": "DefaultSolution",
                "Sync": [
                  {
                    "Type": "Plugin",
                    "AssemblyPath": "default.dll"
                  }
                ]
              },
              {
                "Name": "dev",
                "SolutionName": "DevSolution",
                "Sync": [
                  {
                    "Type": "Plugin",
                    "AssemblyPath": "dev.dll"
                  }
                ]
              }
            ]
          }
        }
        """;

		var tempFile = Path.GetTempFileName();
		File.WriteAllText(tempFile, configJson);

		try
		{
			var configReader = new TestConfigReader(tempFile);
			var builder = new XrmSyncConfigurationBuilder(configReader.GetConfiguration());

			// Act
			var profile = builder.GetProfile("dev");

			// Assert
			Assert.NotNull(profile);
			Assert.Equal("dev", profile.Name);
			Assert.Single(profile.Sync);
			var pluginSync = Assert.IsType<PluginSyncItem>(profile.Sync[0]);
			Assert.Equal("dev.dll", pluginSync.AssemblyPath);
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void ResolveConfigurationNameWithMultipleProfilesAndNoSpecificNameThrowsException()
	{
		// Arrange
		const string configJson = """
        {
          "XrmSync": {
            "DryRun": false,
            "LogLevel": "Information",
            "CiMode": false,
            "Profiles": [
              {
                "Name": "profile1",
                "SolutionName": "Solution1",
                "Sync": [
                  {
                    "Type": "Plugin",
                    "AssemblyPath": "profile1.dll"
                  }
                ]
              },
              {
                "Name": "profile2",
                "SolutionName": "Solution2",
                "Sync": [
                  {
                    "Type": "Plugin",
                    "AssemblyPath": "profile2.dll"
                  }
                ]
              }
            ]
          }
        }
        """;

		var tempFile = Path.GetTempFileName();
		File.WriteAllText(tempFile, configJson);

		try
		{
			var configReader = new TestConfigReader(tempFile);
			var builder = new XrmSyncConfigurationBuilder(configReader.GetConfiguration());

			// Act & Assert
			var exception = Assert.Throws<XrmSync.Model.Exceptions.XrmSyncException>(() => builder.GetProfile(null));
			Assert.Contains("Multiple profiles found", exception.Message);
			Assert.Contains("--profile", exception.Message);
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
            "DryRun": false,
            "LogLevel": "Information",
            "CiMode": false,
            "Profiles": [
              {
                "Name": "myconfig",
                "SolutionName": "MySolution",
                "Sync": [
                  {
                    "Type": "Plugin",
                    "AssemblyPath": "myconfig.dll"
                  }
                ]
              }
            ]
          }
        }
        """;

		var tempFile = Path.GetTempFileName();
		File.WriteAllText(tempFile, configJson);

		try
		{
			var configReader = new TestConfigReader(tempFile);
			var builder = new XrmSyncConfigurationBuilder(configReader.GetConfiguration());

			// Act
			var profile = builder.GetProfile(null);

			// Assert
			Assert.NotNull(profile);
			Assert.Equal("myconfig", profile.Name);
			Assert.Single(profile.Sync);
			var pluginSync = Assert.IsType<PluginSyncItem>(profile.Sync[0]);
			Assert.Equal("myconfig.dll", pluginSync.AssemblyPath);
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void ResolveConfigurationNameWithMultipleProfilesAndDefaultProfileReturnsDefault()
	{
		// Arrange
		const string configJson = """
        {
          "XrmSync": {
            "DryRun": false,
            "LogLevel": "Information",
            "CiMode": false,
            "Profiles": [
              {
                "Name": "default",
                "SolutionName": "DefaultSolution",
                "Sync": [
                  {
                    "Type": "Plugin",
                    "AssemblyPath": "default.dll"
                  }
                ]
              },
              {
                "Name": "dev",
                "SolutionName": "DevSolution",
                "Sync": [
                  {
                    "Type": "Plugin",
                    "AssemblyPath": "dev.dll"
                  }
                ]
              }
            ]
          }
        }
        """;

		var tempFile = Path.GetTempFileName();
		File.WriteAllText(tempFile, configJson);

		try
		{
			var configReader = new TestConfigReader(tempFile);
			var builder = new XrmSyncConfigurationBuilder(configReader.GetConfiguration());

			// Act
			var profile = builder.GetProfile(null);

			// Assert
			Assert.NotNull(profile);
			Assert.Equal("default", profile.Name);
			Assert.Equal("DefaultSolution", profile.SolutionName);
			Assert.Single(profile.Sync);
			var pluginSync = Assert.IsType<PluginSyncItem>(profile.Sync[0]);
			Assert.Equal("default.dll", pluginSync.AssemblyPath);
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void WebresourceSyncItemParsesFileExtensionsFromConfig()
	{
		// Arrange
		const string configJson = """
        {
          "XrmSync": {
            "Profiles": [
              {
                "Name": "default",
                "SolutionName": "TestSolution",
                "Sync": [
                  {
                    "Type": "Webresource",
                    "FolderPath": "wwwroot",
                    "FileExtensions": ["js", "css"]
                  }
                ]
              }
            ]
          }
        }
        """;

		var tempFile = Path.GetTempFileName();
		File.WriteAllText(tempFile, configJson);

		try
		{
			var configReader = new TestConfigReader(tempFile);
			var builder = new XrmSyncConfigurationBuilder(configReader.GetConfiguration());

			// Act
			var profile = builder.GetProfile("default");

			// Assert
			Assert.NotNull(profile);
			var webresourceSync = Assert.IsType<WebresourceSyncItem>(profile.Sync[0]);
			Assert.Equal("wwwroot", webresourceSync.FolderPath);
			Assert.NotNull(webresourceSync.FileExtensions);
			Assert.Equal(["js", "css"], webresourceSync.FileExtensions);
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void WebresourceSyncItemWithoutFileExtensionsDefaultsToNull()
	{
		// Arrange
		const string configJson = """
        {
          "XrmSync": {
            "Profiles": [
              {
                "Name": "default",
                "SolutionName": "TestSolution",
                "Sync": [
                  {
                    "Type": "Webresource",
                    "FolderPath": "wwwroot"
                  }
                ]
              }
            ]
          }
        }
        """;

		var tempFile = Path.GetTempFileName();
		File.WriteAllText(tempFile, configJson);

		try
		{
			var configReader = new TestConfigReader(tempFile);
			var builder = new XrmSyncConfigurationBuilder(configReader.GetConfiguration());

			// Act
			var profile = builder.GetProfile("default");

			// Assert
			Assert.NotNull(profile);
			var webresourceSync = Assert.IsType<WebresourceSyncItem>(profile.Sync[0]);
			Assert.Equal("wwwroot", webresourceSync.FolderPath);
			Assert.Null(webresourceSync.FileExtensions);
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void IdentityRemoveSyncItemParsesFromConfig()
	{
		// Arrange
		const string configJson = """
        {
          "XrmSync": {
            "Profiles": [
              {
                "Name": "default",
                "SolutionName": "TestSolution",
                "Sync": [
                  {
                    "Type": "Identity",
                    "Operation": "Remove",
                    "AssemblyPath": "plugins.dll"
                  }
                ]
              }
            ]
          }
        }
        """;

		var tempFile = Path.GetTempFileName();
		File.WriteAllText(tempFile, configJson);

		try
		{
			var configReader = new TestConfigReader(tempFile);
			var builder = new XrmSyncConfigurationBuilder(configReader.GetConfiguration());

			// Act
			var profile = builder.GetProfile("default");

			// Assert
			Assert.NotNull(profile);
			var identitySync = Assert.IsType<IdentitySyncItem>(profile.Sync[0]);
			Assert.Equal(IdentityOperation.Remove, identitySync.Operation);
			Assert.Equal("plugins.dll", identitySync.AssemblyPath);
			Assert.Null(identitySync.ClientId);
			Assert.Null(identitySync.TenantId);
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void IdentityEnsureSyncItemParsesFromConfig()
	{
		// Arrange
		const string configJson = """
        {
          "XrmSync": {
            "Profiles": [
              {
                "Name": "default",
                "SolutionName": "TestSolution",
                "Sync": [
                  {
                    "Type": "Identity",
                    "Operation": "Ensure",
                    "AssemblyPath": "plugins.dll",
                    "ClientId": "d3b5e6a1-2c4f-4a8b-9e1d-7f3c6b8a2e4d",
                    "TenantId": "a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d"
                  }
                ]
              }
            ]
          }
        }
        """;

		var tempFile = Path.GetTempFileName();
		File.WriteAllText(tempFile, configJson);

		try
		{
			var configReader = new TestConfigReader(tempFile);
			var builder = new XrmSyncConfigurationBuilder(configReader.GetConfiguration());

			// Act
			var profile = builder.GetProfile("default");

			// Assert
			Assert.NotNull(profile);
			var identitySync = Assert.IsType<IdentitySyncItem>(profile.Sync[0]);
			Assert.Equal(IdentityOperation.Ensure, identitySync.Operation);
			Assert.Equal("plugins.dll", identitySync.AssemblyPath);
			Assert.Equal("d3b5e6a1-2c4f-4a8b-9e1d-7f3c6b8a2e4d", identitySync.ClientId);
			Assert.Equal("a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d", identitySync.TenantId);
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void IdentitySyncItemOperationIsCaseInsensitive()
	{
		// Arrange
		const string configJson = """
        {
          "XrmSync": {
            "Profiles": [
              {
                "Name": "default",
                "SolutionName": "TestSolution",
                "Sync": [
                  {
                    "Type": "Identity",
                    "Operation": "ensure",
                    "AssemblyPath": "plugins.dll",
                    "ClientId": "d3b5e6a1-2c4f-4a8b-9e1d-7f3c6b8a2e4d",
                    "TenantId": "a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d"
                  }
                ]
              }
            ]
          }
        }
        """;

		var tempFile = Path.GetTempFileName();
		File.WriteAllText(tempFile, configJson);

		try
		{
			var configReader = new TestConfigReader(tempFile);
			var builder = new XrmSyncConfigurationBuilder(configReader.GetConfiguration());

			// Act
			var profile = builder.GetProfile("default");

			// Assert
			Assert.NotNull(profile);
			var identitySync = Assert.IsType<IdentitySyncItem>(profile.Sync[0]);
			Assert.Equal(IdentityOperation.Ensure, identitySync.Operation);
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void IdentitySyncItemWithMissingOperationIsSkipped()
	{
		// Arrange
		const string configJson = """
        {
          "XrmSync": {
            "Profiles": [
              {
                "Name": "default",
                "SolutionName": "TestSolution",
                "Sync": [
                  {
                    "Type": "Identity",
                    "AssemblyPath": "plugins.dll"
                  }
                ]
              }
            ]
          }
        }
        """;

		var tempFile = Path.GetTempFileName();
		File.WriteAllText(tempFile, configJson);

		try
		{
			var configReader = new TestConfigReader(tempFile);
			var builder = new XrmSyncConfigurationBuilder(configReader.GetConfiguration());

			// Act
			var profile = builder.GetProfile("default");

			// Assert
			Assert.NotNull(profile);
			Assert.Empty(profile.Sync);
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void IdentitySyncItemWithInvalidOperationIsSkipped()
	{
		// Arrange
		const string configJson = """
        {
          "XrmSync": {
            "Profiles": [
              {
                "Name": "default",
                "SolutionName": "TestSolution",
                "Sync": [
                  {
                    "Type": "Identity",
                    "Operation": "InvalidOp",
                    "AssemblyPath": "plugins.dll"
                  }
                ]
              }
            ]
          }
        }
        """;

		var tempFile = Path.GetTempFileName();
		File.WriteAllText(tempFile, configJson);

		try
		{
			var configReader = new TestConfigReader(tempFile);
			var builder = new XrmSyncConfigurationBuilder(configReader.GetConfiguration());

			// Act
			var profile = builder.GetProfile("default");

			// Assert
			Assert.NotNull(profile);
			Assert.Empty(profile.Sync);
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
