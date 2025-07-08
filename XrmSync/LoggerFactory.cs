using Microsoft.Extensions.Logging;

namespace XrmSync;

internal static class LoggerFactory
{
    public static LogLevel MinimumLevel { get; set; } = LogLevel.Information;

    public static ILogger GetLogger<T>()
    {
        var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
        {
            builder.AddFilter("Microsoft", LogLevel.Warning)
                   .AddFilter("System", LogLevel.Warning)
                   .AddFilter("DG", MinimumLevel)
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
