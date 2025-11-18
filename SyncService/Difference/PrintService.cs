using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq.Expressions;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.SyncService.Extensions;

namespace XrmSync.SyncService.Difference;

internal class PrintService(
    ILogger<PrintService> log,
    IOptions<ExecutionModeOptions> configuration,
    IDescription description,
    IDataverseReader dataverseReader
    ) : IPrintService
{
    private readonly LogLevel LogLevel = configuration.Value.DryRun ? LogLevel.Information : LogLevel.Debug;

    public void PrintHeader(PrintHeaderOptions options)
    {
        log.LogInformation("{header}", description.ToolHeader);

        if (configuration.Value.DryRun)
        {
            log.LogInformation("***** DRY RUN *****");
            log.LogInformation("No changes will be made to Dataverse.");
        }

        if (!string.IsNullOrWhiteSpace(options.Message))
        {
            log.LogInformation("{message}", options.Message);
        }

        if (options.PrintConnection)
        {
            log.LogInformation("Connected to Dataverse at {dataverseUrl}", dataverseReader.ConnectedHost);
        }
    }

    public void Print<TEntity>(Difference<TEntity> differences, string title, Func<TEntity, string> namePicker)
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

        Print(wrappedDifference, title, r => namePicker(r.Entity));
    }

    public void Print<TEntity, TParent>(Difference<TEntity, TParent> differences,
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
                log.Log(LogLevel, "  - {name} (recreate) ({props})", namePicker(x.Local), props);
            }
            else
            {
                log.Log(LogLevel, "  - {name}", namePicker(x.Local));
            }
        });

        log.LogInformation("{title} to update: {count}", title, differences.Updates.Count);
        differences.Updates.ForEach(x =>
        {
            var props = GetPropNames(x.Local.Entity, x.Remote?.Entity, x.DifferentProperties);
            log.Log(LogLevel, "  - {name} ({props})", namePicker(x.Local), props);
        });

        log.LogInformation("{title} to delete: {count}", title, differences.Deletes.Count);
        differences.Deletes.ForEach(x => log.Log(LogLevel, "  - {name}", namePicker(x)));
    }

    private static string GetPropNames<TEntity>(TEntity localEntity, TEntity? remoteEntity, IEnumerable<Expression<Func<TEntity, object?>>> differentProperties)
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
}
