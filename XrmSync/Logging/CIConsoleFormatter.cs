using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Options;

namespace XrmSync.Logging;

internal class CIConsoleFormatter : ConsoleFormatter, IDisposable
{
    private readonly IDisposable? _optionsReloadToken;
    private CIConsoleFormatterOptions _formatterOptions;

    public CIConsoleFormatter(IOptionsMonitor<CIConsoleFormatterOptions> options)
        : base("ci-console")
    {
        _optionsReloadToken = options.OnChange(ReloadLoggerOptions);
        _formatterOptions = options.CurrentValue;
    }

    private void ReloadLoggerOptions(CIConsoleFormatterOptions options)
    {
        _formatterOptions = options;
    }

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        var message = logEntry.Formatter(logEntry.State, logEntry.Exception);
        if (message == null) return;

        // Check if CI mode is enabled and this is a warning or error
        var ciPrefix = logEntry.LogLevel switch
        {
            LogLevel.Warning when _formatterOptions.CIMode => "##[warning]",
            LogLevel.Error when _formatterOptions.CIMode => "##[error]",
            LogLevel.Critical when _formatterOptions.CIMode => "##[error]", // Treat critical as error for CI purposes
            _ => null
        };

        // Write CI prefix first if needed
        if (ciPrefix != null)
        {
            textWriter.Write(ciPrefix);
            textWriter.Write(' ');
        }

        // Write timestamp if configured
        var timestampFormat = _formatterOptions.TimestampFormat;
        if (!string.IsNullOrEmpty(timestampFormat))
        {
            var timestamp = GetCurrentDateTime().ToString(timestampFormat);
            textWriter.Write(timestamp);
        }

        // Write loglevel
        textWriter.Write(GetColorizedLogLevelString(logEntry.LogLevel));
        textWriter.Write(' ');

        // Write category
        textWriter.Write(logEntry.Category);
        textWriter.Write(' ');

        // Write the message
        textWriter.Write(message);

        // Write exception if present
        if (logEntry.Exception != null)
        {
            textWriter.Write(' ');
            textWriter.Write(logEntry.Exception.ToString());
        }

        // Write newline
        textWriter.WriteLine();
    }

    private string GetColorizedLogLevelString(LogLevel logLevel)
    {
        // Apply colors if needed
        var consoleColors = GetLogLevelConsoleColors(logLevel);

        var fgColor = consoleColors.Foreground.HasValue
            ? $"\u001b[38;5;{(int)consoleColors.Foreground.Value}m" : string.Empty;
        var bgColor = consoleColors.Background.HasValue
            ? $"\u001b[48;5;{(int)consoleColors.Background.Value}m" : string.Empty;
        var esc = (consoleColors.Foreground.HasValue || consoleColors.Background.HasValue)
            ? "\u001b[0m" : string.Empty;

        var level = GetLogLevelString(logLevel);

        return $"{fgColor}{bgColor}{level}{esc}";
    }

    private static string GetLogLevelString(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "trce",
            LogLevel.Debug => "dbug",
            LogLevel.Information => "info",
            LogLevel.Warning => "warn",
            LogLevel.Error => "fail",
            LogLevel.Critical => "crit",
            _ => throw new ArgumentOutOfRangeException(nameof(logLevel))
        };
    }

    private DateTimeOffset GetCurrentDateTime()
    {
        return _formatterOptions.UseUtcTimestamp ? DateTimeOffset.UtcNow : DateTimeOffset.Now;
    }

    private ConsoleColors GetLogLevelConsoleColors(LogLevel logLevel)
    {
        // We shouldn't be outputting color codes for Android/Apple mobile platforms,
        // they have no shell (adb shell is not meant for running apps) and all the output gets redirected to some log file.
        bool disableColors = _formatterOptions.ColorBehavior == LoggerColorBehavior.Disabled || _formatterOptions.CIMode;
        if (disableColors)
        {
            return new ConsoleColors(null, null);
        }
        // We must explicitly set the background color if we are setting the foreground color,
        // since just setting one can look bad on the users console.
        return logLevel switch
        {
            LogLevel.Trace => new ConsoleColors(ConsoleColor.DarkGray, ConsoleColor.Black),
            LogLevel.Debug => new ConsoleColors(ConsoleColor.Gray, ConsoleColor.Black),
            LogLevel.Information => new ConsoleColors(ConsoleColor.DarkGreen, ConsoleColor.Black),
            LogLevel.Warning => new ConsoleColors(ConsoleColor.Yellow, ConsoleColor.Black),
            LogLevel.Error => new ConsoleColors(ConsoleColor.Black, ConsoleColor.DarkRed),
            LogLevel.Critical => new ConsoleColors(ConsoleColor.White, ConsoleColor.DarkRed),
            _ => new ConsoleColors(null, null)
        };
    }

    public void Dispose()
    {
        _optionsReloadToken?.Dispose();
    }

    private record ConsoleColors(ConsoleColor? Foreground, ConsoleColor? Background);
}