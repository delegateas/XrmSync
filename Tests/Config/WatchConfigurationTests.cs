using Microsoft.Extensions.Configuration;
using XrmSync.Model;
using XrmSync.Options;

namespace Tests.Config;

public class WatchConfigurationTests
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
	public void PerItemWatchFlagAndGlobalDebounceAreBound()
	{
		// Arrange
		const string configJson = """
        {
          "XrmSync": {
            "WatchDebounceMs": 250,
            "Profiles": [
              {
                "Name": "default",
                "SolutionName": "MySolution",
                "AssemblyPath": "a.dll",
                "Sync": [
                  { "Type": "Plugin", "Watch": true },
                  { "Type": "Webresource", "FolderPath": "wwwroot", "Watch": true },
                  { "Type": "PluginAnalysis", "PublisherPrefix": "new" }
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
		Assert.Equal(250, config.WatchDebounceMs);
		Assert.True(Assert.IsType<PluginSyncItem>(profile.Sync[0]).Watch);
		Assert.True(Assert.IsType<WebresourceSyncItem>(profile.Sync[1]).Watch);
		Assert.False(Assert.IsType<PluginAnalysisSyncItem>(profile.Sync[2]).Watch);
	}

	[Fact]
	public void WatchDefaultsToOffAndDebounceToItsDefaultValue()
	{
		// Arrange
		const string configJson = """
        {
          "XrmSync": {
            "Profiles": [
              {
                "Name": "default",
                "SolutionName": "MySolution",
                "Sync": [
                  { "Type": "Plugin", "AssemblyPath": "a.dll" }
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
		Assert.Equal(XrmSyncConfiguration.DefaultWatchDebounceMs, config.WatchDebounceMs);
		Assert.False(Assert.IsType<PluginSyncItem>(profile.Sync[0]).Watch);
	}

	[Theory]
	[InlineData(49, false)]
	[InlineData(50, true)]
	[InlineData(500, true)]
	[InlineData(60_000, true)]
	[InlineData(60_001, false)]
	public void WatchDebounceIsValidatedAgainstItsAllowedRange(int milliseconds, bool expectedValid)
	{
		// Act
		var errors = XrmSyncConfigurationValidator.ValidateWatchDebounce(milliseconds).ToList();

		// Assert
		if (expectedValid)
		{
			Assert.Empty(errors);
		}
		else
		{
			Assert.Contains("WatchDebounceMs", Assert.Single(errors));
		}
	}
}
