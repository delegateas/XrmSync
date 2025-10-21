using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System.Runtime.CompilerServices;
using XrmSync.Dataverse.Extensions;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Exceptions;

namespace XrmSync.Dataverse;

internal class DryRunDataverseWriter : IDataverseWriter
{
    private readonly ILogger<DryRunDataverseWriter> logger;

    public DryRunDataverseWriter(IOptions<ExecutionOptions> configuration, ILogger<DryRunDataverseWriter> logger)
    {
        if (!configuration.Value.DryRun)
        {
            throw new XrmSyncException("This writer is intended for dry runs only.");
        }

        this.logger = logger;
    }

    public Guid Create(Entity entity, IDictionary<string, object>? parameters = null)
    {
        LogOperation(entity, parameters);

        return Guid.NewGuid(); // In dry run mode, we do not actually create the entity.
    }

    private void PerformAsBulk<T>(IEnumerable<T> updates) where T : OrganizationRequest
    {
        List<T> updateList = [.. updates];
        var targetTypes = updateList.Select(t =>
            t switch {
                CreateRequest cr => cr.Target.LogicalName,
                UpdateRequest ur => ur.Target.LogicalName,
                DeleteRequest dr => dr.Target.LogicalName,
                _ => throw new XrmSyncException($"Unexpected request type: {typeof(T)}, expected Create, Update or Delete request")
            }).Distinct().ToList();

        logger.LogTrace("Would execute {count} {type} requests targeting entities of type {target}", updateList.Count, typeof(T).Name, string.Join(", ", targetTypes));
    }

    public void Update(Entity entity)
    {
        LogOperation(entity);
    }

    public void UpdateMultiple<TEntity>(IEnumerable<TEntity> entities) where TEntity : Entity
    {
        PerformAsBulk(entities.Select(e => new UpdateRequest { Target = e }));
    }

    public void DeleteMultiple<TEntity>(IEnumerable<TEntity> entities) where TEntity : Entity
        => DeleteMultiple(entities.ToDeleteRequests());

    public void DeleteMultiple(IEnumerable<DeleteRequest> deleteRequests)
    {
        PerformAsBulk([.. deleteRequests]);
    }

    private void LogOperation(Entity entity, IDictionary<string, object>? parameters = null, [CallerMemberName] string operation = "")
    {
        logger.LogTrace("{Operation} operation would be performed for entity of type '{EntityType}'.",
                        operation, entity.LogicalName);

        logger.LogTrace("{attrs}", string.Join("\n", entity.Attributes.Select(kvp => $" - {kvp.Key}: {TruncateValue(kvp.Value)}")));

        if (parameters?.Count > 0)
        {
            logger.LogTrace("Parameters: {params}", string.Join(", ", parameters.Select(kvp => $"{kvp.Key}: {TruncateValue(kvp.Value)}")));
        }
    }

    // Helper method to truncate long values for logging
    private static string TruncateValue(object value)
    {
        if (value == null) return "<null>";
        string str = value.ToString() ?? "<null>";
        const int maxLen = 120;
        if (str.Length <= maxLen)
            return str;
        int extraBytes = System.Text.Encoding.UTF8.GetByteCount(str) - System.Text.Encoding.UTF8.GetByteCount(str.Substring(0, maxLen));
        return $"{str.Substring(0, maxLen)}... (+{extraBytes} bytes more)";
    }
}
