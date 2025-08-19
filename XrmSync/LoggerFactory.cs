using Microsoft.Extensions.Logging;

namespace XrmSync;

internal static class LoggerFactory
{
    public const LogLevel DefaultLogLevel = LogLevel.Information;

    public static ILogger CreateLogger<T>(LogLevel? logLevel)
    {
        var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.AddFilter(nameof(Microsoft), LogLevel.Warning)
                .AddFilter(nameof(System), LogLevel.Warning)
                .AddFilter(nameof(XrmSync), logLevel ?? DefaultLogLevel)
                .AddSimpleConsole(options =>
                {
                    options.IncludeScopes = false;
                    options.SingleLine = true;
                    options.TimestampFormat = "hh:mm:ss ";
                });
        });

        return loggerFactory.CreateLogger<T>();
    }
}
