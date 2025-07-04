using DG.XrmPluginSync.SyncService.Common;
using Microsoft.Extensions.Logging;

namespace DG.XrmPluginSync.SyncService.Extensions;

internal static class LoggerExtensions
{
    public static void Print<T>(this ILogger log, Difference<T> differences, string title, Func<T, string> namePicker)
    {
        log.LogInformation("{title} to create: {count}", title, differences.Creates.Count);
        differences.Creates.ForEach(x => log.LogDebug("  - {name}", namePicker(x)));

        log.LogInformation("{title} to update: {count}", title, differences.Updates.Count);
        differences.Updates.ForEach(x => log.LogDebug("  - {name}", namePicker(x)));

        log.LogInformation("{title} to delete: {count}", title,differences.Deletes.Count);
        differences.Deletes.ForEach(x => log.LogDebug("  - {name}", namePicker(x)));
    }
}
