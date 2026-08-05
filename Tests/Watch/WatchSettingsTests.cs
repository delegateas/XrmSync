using Microsoft.Extensions.Logging;
using XrmSync.Model;
using XrmSync.Watch;

namespace Tests.Watch;

public class WatchSettingsTests
{
	private static XrmSyncConfiguration Config(int debounceMs = XrmSyncConfiguration.DefaultWatchDebounceMs) =>
		new(DryRun: false, LogLevel: LogLevel.Information, CiMode: false, Profiles: [], WatchDebounceMs: debounceMs);

	[Theory]
	[InlineData(true, false, true)]   // --watch enables watching even without per-item flags
	[InlineData(null, true, true)]    // a per-item Watch flag is enough on its own
	[InlineData(false, true, false)]  // --watch false overrides the per-item flags
	[InlineData(null, false, false)]  // nothing requested watching
	public void CliFlagOverridesThePerItemFlags(bool? cliWatch, bool anyItemWatch, bool expectedEnabled)
	{
		// Act
		var settings = WatchSettings.Resolve(cliWatch, anyItemWatch, ciMode: false, Config());

		// Assert
		Assert.Equal(expectedEnabled, settings.Enabled);
		Assert.False(settings.Suppressed);
	}

	[Fact]
	public void CiModeSuppressesWatchingSoAPipelineCannotHang()
	{
		// Act
		var settings = WatchSettings.Resolve(cliWatch: true, anyItemWatch: true, ciMode: true, Config());

		// Assert
		Assert.False(settings.Enabled);
		Assert.True(settings.Suppressed);
	}

	[Fact]
	public void CiModeWithoutAWatchRequestIsNotReportedAsSuppressed()
	{
		// Act
		var settings = WatchSettings.Resolve(cliWatch: null, anyItemWatch: false, ciMode: true, Config());

		// Assert
		Assert.False(settings.Enabled);
		Assert.False(settings.Suppressed);
	}

	[Theory]
	[InlineData(500, 500)]
	[InlineData(2000, 2000)]
	[InlineData(10, 50)]        // clamped up, so the loop cannot spin
	[InlineData(120_000, 60_000)] // clamped down
	public void DebounceComesFromTheConfiguredValueClampedToTheValidRange(int configured, int expectedMs)
	{
		// Act
		var settings = WatchSettings.Resolve(cliWatch: true, anyItemWatch: false, ciMode: false, Config(configured));

		// Assert
		Assert.Equal(TimeSpan.FromMilliseconds(expectedMs), settings.Debounce);
	}
}
