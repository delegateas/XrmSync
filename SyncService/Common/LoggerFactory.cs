using Microsoft.Extensions.Logging;
using System.Reflection;
using DG.XrmPluginSync.SyncService.Models.Requests;

namespace DG.XrmPluginSync.SyncService.Common;

internal static class LoggerFactory
{
    public static ILogger GetLogger<T>()
    {
        var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder => {
            builder.AddFilter("Microsoft", LogLevel.Warning)
                   .AddFilter("System", LogLevel.Warning)
                   .AddFilter("DG.Daxif", LogLevel.Trace)
                   .AddSimpleConsole(options =>
                   {
                       options.IncludeScopes = false;
                       options.SingleLine = true;
                       options.TimestampFormat = "hh:mm:ss ";
                   });
            });
        return loggerFactory.CreateLogger<T>();
    }

    public static void LogAndValidateRequest(this ILogger log, IRequest request)
    {
        log.LogInformation(InternalUtility.daxifVersion);
        log.LogInformation(request.GetName());
        if (request.GetArguments().Count != 0)
        {
            foreach (var (key, value) in request.GetArguments())
            {
                log.LogTrace($"{key}: {value}");
            }
        }
        try
        {
            request.Validate();
        } 
        catch (AggregateException e)
        {
            log.LogError(e.Message);
            foreach(var ex in e.InnerExceptions)
            {
                log.LogError(ex.Message);
            }
            throw;
        } 
        catch (Exception e)
        {
            log.LogError(e.Message);
            throw;
        }
    }

    public static void LogRequest(this ILogger log, Uri envUrl, string callName, List<KeyValuePair<string, string>> arguments = null)
    {
        log.LogInformation(InternalUtility.daxifVersion);
        log.LogInformation(callName);
        if (envUrl != null) log.LogTrace($"Organization: {envUrl}");
        if (arguments != null)
        {
            foreach (var arg in arguments)
            {
                log.LogTrace($"{arg.Key}: {arg.Value}");
            }
        }
    }

    public static string GetSyncDescription()
    {
        return $"Synced with DAPPIF# " +
        $"v.{Assembly.GetExecutingAssembly().GetName().Version.ToString()} " +
        $"by '{Environment.UserName}' " +
        $"at {DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss \"GMT\"zzz")}";
    }
}
