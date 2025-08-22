using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.SyncService.Difference;

namespace XrmSync.SyncService.Extensions;

internal static class LoggerExtensions
{
    public static void Print<TEntity>(this ILogger log, Difference<TEntity> differences, string title, Func<TEntity, string> namePicker) where TEntity : EntityBase
    {
        log.LogInformation("{title} to create: {count}", title, differences.Creates.Count);
        differences.Creates.ForEach(x =>
        {
            var props = GetPropNames(x);
            if (!string.IsNullOrEmpty(props))
            {
                log.LogDebug("  - {name} (recreate) ({props})", namePicker(x.LocalEntity), props);
            } else
            {
                log.LogDebug("  - {name}", namePicker(x.LocalEntity));
            }
        });

        log.LogInformation("{title} to update: {count}", title, differences.Updates.Count);
        differences.Updates.ForEach(x =>
        {
            var props = GetPropNames(x);
            log.LogDebug("  - {name} ({props})", namePicker(x.LocalEntity), props);
        });

        log.LogInformation("{title} to delete: {count}", title,differences.Deletes.Count);
        differences.Deletes.ForEach(x => log.LogDebug("  - {name}", namePicker(x)));
    }

    private static string GetPropNames<TEntity>(EntityDifference<TEntity> diff) where TEntity : EntityBase
    {
        return string.Join(", ", diff.DifferentProperties
            .Select(p =>
            {
                var memberName = p.GetMemberName();
                var propGetter = p.Compile();
                var localValue = propGetter(diff.LocalEntity);
                var remoteValue = diff.RemoteEntity != null ? propGetter(diff.RemoteEntity) : null;

                return $"{memberName}: \"{remoteValue ?? "<null>"}\" -> \"{localValue ?? "<null>"}\"";
            }));
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
