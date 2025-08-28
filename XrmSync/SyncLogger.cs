﻿using Microsoft.Extensions.Logging;
using XrmSync.Model;

namespace XrmSync;

internal class SyncLogger<T> : ILogger<T>
{
    private readonly ILogger _logger;

    public SyncLogger(ILoggerFactory loggerFactory, XrmSyncConfiguration configuration)
    {
        LogLevel minLogLevel = configuration.Plugin?.Sync?.LogLevel is LogLevel level
            ? level
            : LogLevel.Information;

        string categoryName = minLogLevel < LogLevel.Information
            ? typeof(T).FullName ?? typeof(T).Namespace?.Split('.').First() ?? typeof(T).Name
            : typeof(T).Namespace?.Split('.').First() ?? typeof(T).Name;

        _logger = loggerFactory.CreateLogger(categoryName);
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => _logger.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel) => _logger.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        _logger.Log(logLevel, eventId, state, exception, formatter);
    }
}
