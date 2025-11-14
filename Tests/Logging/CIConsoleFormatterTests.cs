using Microsoft.Extensions.Options;
using XrmSync.Logging;

namespace Tests.Logging;

public class CIConsoleFormatterTests
{
    private class TestOptionsMonitor<T> : IOptionsMonitor<T>
    {
        public TestOptionsMonitor(T currentValue)
        {
            CurrentValue = currentValue;
        }

        public T CurrentValue { get; }

        public T Get(string? name) => CurrentValue;

        public IDisposable OnChange(Action<T, string?> listener) => new TestDisposable();
    }

    private class TestDisposable : IDisposable
    {
        public void Dispose() { }
    }

    [Fact]
    public void CIConsoleFormatterWithCIModeEnabledDetectsFromOptions()
    {
        // Arrange
        var options = new CIConsoleFormatterOptions
        {
            IncludeScopes = false,
            SingleLine = true,
            TimestampFormat = "HH:mm:ss ",
            CIMode = true
        };
        var optionsMonitor = new TestOptionsMonitor<CIConsoleFormatterOptions>(options);
        var formatter = new CIConsoleFormatter(optionsMonitor);

        // Act - Test CI mode detection
        var ciMode = GetCIModeFromFormatter(formatter);

        // Assert
        Assert.True(ciMode);
    }

    [Fact]
    public void CIConsoleFormatterWithCIModeDisabledDetectsFromOptions()
    {
        // Arrange
        var options = new CIConsoleFormatterOptions
        {
            IncludeScopes = false,
            SingleLine = true,
            TimestampFormat = "HH:mm:ss ",
            CIMode = false
        };
        var optionsMonitor = new TestOptionsMonitor<CIConsoleFormatterOptions>(options);
        var formatter = new CIConsoleFormatter(optionsMonitor);

        // Act - Test CI mode detection
        var ciMode = GetCIModeFromFormatter(formatter);

        // Assert
        Assert.False(ciMode);
    }

    [Fact]
    public void CIConsoleFormatterOptionsDefaultsToFalse()
    {
        // Arrange
        var options = new CIConsoleFormatterOptions
        {
            IncludeScopes = false,
            SingleLine = true,
            TimestampFormat = "HH:mm:ss "
            // CIMode not explicitly set
        };

        // Act & Assert
        Assert.False(options.CIMode);
    }

    private bool GetCIModeFromFormatter(CIConsoleFormatter formatter)
    {
        // Use reflection to test the private _formatterOptions field
        var field = typeof(CIConsoleFormatter).GetField("_formatterOptions", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var formatterOptions = (CIConsoleFormatterOptions?)field?.GetValue(formatter);
        return formatterOptions?.CIMode ?? false;
    }
}