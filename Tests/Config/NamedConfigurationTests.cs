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
