using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XrmSync.Logging;
using XrmSync.Model;

namespace Tests.Logging;

public class SyncLoggerTests
{
    private class TestLogger : ILogger
    {
        public List<(LogLevel Level, object? State, Type? StateType, string Message)> LoggedMessages { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            LoggedMessages.Add((logLevel, state, state?.GetType(), formatter(state, exception)));
        }
    }

    private class TestLoggerFactory : ILoggerFactory
    {
        public TestLogger Logger { get; } = new();

        public void AddProvider(ILoggerProvider provider) { }

        public ILogger CreateLogger(string categoryName) => Logger;

        public void Dispose() { }
    }

    [Fact]
    public void Log_PassesThroughDirectly()
    {
        // Arrange
        var loggerFactory = new TestLoggerFactory();
        var config = new XrmSyncConfiguration(false, LogLevel.Information, false, new List<ProfileConfiguration>());
        var syncLogger = new SyncLogger<SyncLoggerTests>(loggerFactory, Options.Create(config));

        // Act
        syncLogger.LogWarning("This is a warning message");

        // Assert
        var logEntry = Assert.Single(loggerFactory.Logger.LoggedMessages);
        Assert.Equal(LogLevel.Warning, logEntry.Level);
        Assert.Equal("This is a warning message", logEntry.Message);
        
        // State should pass through directly - CI mode handling is now in the formatter
        Assert.NotNull(logEntry.StateType);
    }

    [Fact]
    public void Log_AllLogLevels_PassThroughDirectly()
    {
        // Arrange
        var loggerFactory = new TestLoggerFactory();
        var config = new XrmSyncConfiguration(false, LogLevel.Information, false, new List<ProfileConfiguration>());
        var syncLogger = new SyncLogger<SyncLoggerTests>(loggerFactory, Options.Create(config));

        // Act
        syncLogger.LogWarning("This is a warning message");
        syncLogger.LogError("This is an error message");
        syncLogger.LogInformation("This is an info message");
        syncLogger.LogDebug("This is a debug message");

        // Assert
        var logEntries = loggerFactory.Logger.LoggedMessages;
        Assert.Equal(4, logEntries.Count);
        
        var warningEntry = logEntries.First(x => x.Level == LogLevel.Warning);
        var errorEntry = logEntries.First(x => x.Level == LogLevel.Error);
        var infoEntry = logEntries.First(x => x.Level == LogLevel.Information);
        var debugEntry = logEntries.First(x => x.Level == LogLevel.Debug);
        
        Assert.Equal("This is a warning message", warningEntry.Message);
        Assert.Equal("This is an error message", errorEntry.Message);
        Assert.Equal("This is an info message", infoEntry.Message);
        Assert.Equal("This is a debug message", debugEntry.Message);
    }
}