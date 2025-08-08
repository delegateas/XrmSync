using Microsoft.Extensions.Logging;

namespace XrmSync;

internal static class LoggerFactory
{
    public static ILogger CreateLogger<T>(LogLevel logLevel)
    {
        var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.AddFilter("Microsoft", LogLevel.Warning)
                .AddFilter("System", LogLevel.Warning)
                .AddFilter("XrmSync", logLevel)
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
