using Microsoft.Extensions.Logging;

namespace DG.XrmPluginSync;

internal static class LoggerFactory
{
    public static LogLevel MinimumLevel { get; set; } = LogLevel.Trace;

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
