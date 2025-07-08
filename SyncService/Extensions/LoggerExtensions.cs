using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.SyncService.Differences;

namespace XrmSync.SyncService.Extensions;

internal static class LoggerExtensions
{
    public static void Print<TEntity>(this ILogger log, Difference<TEntity> differences, string title, Func<TEntity, string> namePicker) where TEntity : EntityBase
    {
        log.LogInformation("{title} to create: {count}", title, differences.Creates.Count);
        differences.Creates.ForEach(x => log.LogDebug("  - {name}", namePicker(x)));

        log.LogInformation("{title} to update: {count}", title, differences.Updates.Count);
        differences.UpdatesWithDifferences.ForEach(x =>
        {
            var props = x.DifferentProperties
                .Select(p =>
                {
                    var memberName = p.GetMemberName();
                    var propGetter = p.Compile();
                    var localValue = propGetter(x.LocalEntity);
                    var remoteValue = x.RemoteEntity != null ? propGetter(x.RemoteEntity) : null;

                    return $"{memberName}: \"{remoteValue ?? "<null>"}\" -> \"{localValue ?? "<null>"}\"";
                });
            log.LogDebug("  - {name} ({props})", namePicker(x.LocalEntity), string.Join(", ", props));
        });

        log.LogInformation("{title} to delete: {count}", title,differences.Deletes.Count);
        differences.Deletes.ForEach(x => log.LogDebug("  - {name}", namePicker(x)));
    }

    public static string GetMemberName<T>(this Expression<Func<T, object>> lambda)
    {
        var body = lambda.Body as MemberExpression;
        if (body == null)
        {
            var ubody = lambda.Body as UnaryExpression ?? throw new XrmSyncException("Expression is not a member access");
            body = ubody.Operand as MemberExpression;
        }

        if (body == null)
            throw new XrmSyncException("Expression is not a member access");

        return body.Member.Name;
    }
}
