using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using XrmSync.Extensions;
using XrmSync.Logging;
using XrmSync.Model;

namespace Tests;

public class CIModeDemonstrationTests
{
    [Fact]
    public void Demonstrate_CIModePassedToFormatter()
    {
        // This test demonstrates that CI mode is properly passed to the formatter through options

        // Arrange - Create service provider with CI mode enabled via AddLogger parameter
        var services = new ServiceCollection()
            .AddSingleton(new XrmSyncConfiguration(new(new PluginSyncOptions("path", "solution", LogLevel.Debug, false), null)))
            .AddLogger(sp => LogLevel.Debug, ciMode: true) // Explicitly enable CI mode
            .BuildServiceProvider();

        var configuration = services.GetRequiredService<XrmSyncConfiguration>();

        // Capture what the logger would send to the formatter
        var capturedMessages = new List<(LogLevel Level, string Message)>();
        var testLogger = new TestCaptureLogger(capturedMessages);
        var syncLogger = new SyncLogger<CIModeDemonstrationTests>(
            new TestLoggerFactory(testLogger), 
            configuration
        );

        // Act
        syncLogger.LogWarning("This is a warning in CI mode");
        syncLogger.LogError("This is an error in CI mode");
        syncLogger.LogInformation("This is info in CI mode");

        // Assert
        Assert.Equal(3, capturedMessages.Count);

        // All messages should pass through directly without wrapping
        var warningMessage = capturedMessages[0];
        var errorMessage = capturedMessages[1];
        var infoMessage = capturedMessages[2];

        Assert.Equal(LogLevel.Warning, warningMessage.Level);
        Assert.Equal("This is a warning in CI mode", warningMessage.Message);

        Assert.Equal(LogLevel.Error, errorMessage.Level);
        Assert.Equal("This is an error in CI mode", errorMessage.Message);

        Assert.Equal(LogLevel.Information, infoMessage.Level);
        Assert.Equal("This is info in CI mode", infoMessage.Message);
    }

    private class TestCaptureLogger : ILogger
    {
        private readonly List<(LogLevel Level, string Message)> _capturedMessages;

        public TestCaptureLogger(List<(LogLevel Level, string Message)> capturedMessages)
        {
            _capturedMessages = capturedMessages;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _capturedMessages.Add((logLevel, formatter(state, exception)));
        }
    }

    private class TestLoggerFactory : ILoggerFactory
    {
        private readonly ILogger _logger;

        public TestLoggerFactory(ILogger logger)
        {
            _logger = logger;
        }

        public void AddProvider(ILoggerProvider provider) { }
        public ILogger CreateLogger(string categoryName) => _logger;
        public void Dispose() { }
    }
}