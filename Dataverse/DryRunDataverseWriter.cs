using Microsoft.Extensions.Logging;
using Microsoft.Xrm.Sdk;
using System.Runtime.CompilerServices;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Exceptions;

namespace XrmSync.Dataverse;

public class DryRunDataverseWriter : IDataverseWriter
{
    private readonly ILogger logger;

    public DryRunDataverseWriter(XrmSyncOptions options, ILogger logger)
    {
        if (!options.DryRun)
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

    public List<ExecuteMultipleResponseItem> PerformAsBulk<T>(List<T> updates, Func<T, string> targetSelector) where T : OrganizationRequest
    {
        var targetTypes = updates.Select(targetSelector).Distinct().ToList();
        logger.LogTrace("DRY RUN: Would execute {count} {type} requests targeting entities of type {target}", updates.Count, typeof(T).Name, string.Join(", ", targetTypes));
        return [];
    }

    public void PerformAsBulkWithOutput<T>(List<T> updates, Func<T, string> targetSelector) where T : OrganizationRequest
    {
        PerformAsBulk(updates, targetSelector);
    }

    public void Update(Entity entity)
    {
        LogOperation(entity);
    }

    public void UpdateMultiple<TEntity>(List<TEntity> entities) where TEntity : Entity
    {
        if (entities.Count == 0)
        {
            return;
        }

        logger.LogTrace("DRY RUN: UpdateMultiple operation would be performed for {count} entities of type '{entityType}'.",
                        entities.Count, entities[0].LogicalName);
    }

    private void LogOperation(Entity entity, IDictionary<string, object>? parameters = null, [CallerMemberName] string operation = "")
    {
        logger.LogTrace("DRY RUN: {Operation} operation would be performed for entity of type '{EntityType}'.",
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
