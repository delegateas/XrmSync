using DG.XrmPluginSync.SyncService.Common;
using Microsoft.Extensions.Logging;

namespace DG.XrmPluginSync.SyncService.Extensions;

internal static class LoggerExtensions
{
    public static void Print<T>(this ILogger log, Difference<T> differences, string title, Func<T, string> namePicker)
    {
        log.LogInformation("{0}", title);

        log.LogInformation(" Creates ({0}):", differences.Creates.Count);
        differences.Creates.ForEach(x => log.LogInformation("  - {0}", namePicker(x)));

        log.LogInformation(" Updates ({0}):", differences.Updates.Count);
        differences.Updates.ForEach(x => log.LogInformation("  - {0}", namePicker(x)));

        log.LogInformation(" Deletes ({0}):", differences.Deletes.Count);
        differences.Deletes.ForEach(x => log.LogInformation("  - {0}", namePicker(x)));
    }
}
