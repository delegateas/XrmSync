using Microsoft.Extensions.Configuration;
using XrmSync.Model;
using XrmSync.Options;

namespace Tests.Config;

public class NoDeleteConfigurationTests
{
	private static XrmSyncConfiguration BuildFrom(string configJson)
	{
		var tempFile = Path.GetTempFileName();
		File.WriteAllText(tempFile, configJson);

		try
		{
			var configuration = new ConfigurationBuilder().AddJsonFile(tempFile).Build();
			return new XrmSyncConfigurationBuilder(configuration).Build();
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	[Fact]
	public void PerItemNoDeleteFlagIsBound()
	{
		// Arrange
		const string configJson = """
        {
          "XrmSync": {
            "Profiles": [
              {
                "Name": "default",
                "SolutionName": "MySolution",
                "AssemblyPath": "a.dll",
                "Sync": [
                  { "Type": "Plugin", "NoDelete": true },
                  { "Type": "Webresource", "FolderPath": "wwwroot", "NoDelete": true }
                ]
              }
            ]
          }
        }
        """;

		// Act
		var config = BuildFrom(configJson);
		var profile = Assert.Single(config.Profiles);

		// Assert
		Assert.True(Assert.IsType<PluginSyncItem>(profile.Sync[0]).NoDelete);
		Assert.True(Assert.IsType<WebresourceSyncItem>(profile.Sync[1]).NoDelete);
	}

	[Fact]
	public void NoDeleteDefaultsToOffWhenAbsent()
	{
		// Arrange
		const string configJson = """
        {
          "XrmSync": {
            "Profiles": [
              {
                "Name": "default",
                "SolutionName": "MySolution",
                "AssemblyPath": "a.dll",
                "Sync": [
                  { "Type": "Plugin" },
                  { "Type": "Webresource", "FolderPath": "wwwroot" }
                ]
              }
            ]
          }
        }
        """;

		// Act
		var config = BuildFrom(configJson);
		var profile = Assert.Single(config.Profiles);

		// Assert
		Assert.False(Assert.IsType<PluginSyncItem>(profile.Sync[0]).NoDelete);
		Assert.False(Assert.IsType<WebresourceSyncItem>(profile.Sync[1]).NoDelete);
	}

	[Fact]
	public void NoDeleteIsIndependentOfAllowEmptyTypes()
	{
		// Arrange — the two flags overlap for plugin types but are configured separately
		const string configJson = """
        {
          "XrmSync": {
            "Profiles": [
              {
                "Name": "default",
                "SolutionName": "MySolution",
                "AssemblyPath": "a.dll",
                "Sync": [
                  { "Type": "Plugin", "AllowEmptyTypes": true }
                ]
              }
            ]
          }
        }
        """;

		// Act
		var config = BuildFrom(configJson);
		var profile = Assert.Single(config.Profiles);
		var plugin = Assert.IsType<PluginSyncItem>(Assert.Single(profile.Sync));

		// Assert
		Assert.True(plugin.AllowEmptyTypes);
		Assert.False(plugin.NoDelete);
	}
}
