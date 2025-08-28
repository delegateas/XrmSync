using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.SyncService.Difference;

namespace XrmSync.SyncService.Extensions;

internal static class LoggerExtensions
{
    public static void Print<TEntity>(this ILogger log, Difference<TEntity> differences, string title, Func<TEntity, string> namePicker)
        where TEntity : EntityBase
    {
        var creates = differences.Creates
            .Select(c => EntityDifference<TEntity, TEntity>.FromLocal(new ParentReference<TEntity, TEntity>(c.Local, null!)));
        var updates = differences.Updates
            .Select(u => new EntityDifference<TEntity, TEntity>(new ParentReference<TEntity, TEntity>(u.Local, null!),
                new ParentReference<TEntity, TEntity>(u.Remote!, null!), u.DifferentProperties));
        var deletes = differences.Deletes
            .Select(d => new ParentReference<TEntity, TEntity>(d, null!));

        var wrappedDifference = new Difference<TEntity, TEntity>(
            [.. creates],
            [.. updates],
            [.. deletes]
        );

        log.Print(wrappedDifference, title, r => namePicker(r.Entity));
    }

    public static void Print<TEntity, TParent>(this ILogger log,
        Difference<TEntity, TParent> differences,
        string title,
        Func<ParentReference<TEntity, TParent>, string> namePicker)
        where TEntity : EntityBase
        where TParent : EntityBase
    {
        log.LogInformation("{title} to create: {count}", title, differences.Creates.Count);
        differences.Creates.ForEach(x =>
        {
            var props = GetPropNames(x.Local.Entity, x.Remote?.Entity, x.DifferentProperties);
            if (!string.IsNullOrEmpty(props))
            {
                log.LogDebug("  - {name} (recreate) ({props})", namePicker(x.Local), props);
            }
            else
            {
                log.LogDebug("  - {name}", namePicker(x.Local));
            }
        });

        log.LogInformation("{title} to update: {count}", title, differences.Updates.Count);
        differences.Updates.ForEach(x =>
        {
            var props = GetPropNames(x.Local.Entity, x.Remote?.Entity, x.DifferentProperties);
            log.LogDebug("  - {name} ({props})", namePicker(x.Local), props);
        });

        log.LogInformation("{title} to delete: {count}", title, differences.Deletes.Count);
        differences.Deletes.ForEach(x => log.LogDebug("  - {name}", namePicker(x)));
    }

    private static string GetPropNames<TEntity>(TEntity localEntity, TEntity remoteEntity, IEnumerable<Expression<Func<TEntity, object>>> differentProperties)
    {
        return string.Join(", ", differentProperties
            .Select(p =>
            {
                var memberName = p.GetMemberName();
                var propGetter = p.Compile();
                var localValue = propGetter(localEntity);
                var remoteValue = remoteEntity != null ? propGetter(remoteEntity) : null;

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
