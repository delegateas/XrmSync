using DG.XrmPluginSync.Dataverse.Interfaces;
using DG.XrmPluginSync.Model;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System.Runtime.CompilerServices;

namespace DG.XrmPluginSync.Dataverse;

public class DataverseWriter(ServiceClient serviceClient, ILogger logger, XrmPluginSyncOptions options) : IDataverseWriter
{
    public Guid Create(Entity entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity), "The provided entity cannot be null.");
        }

        if (options.DryRun)
        {
            LogOperation(entity);

            return Guid.NewGuid(); // In dry run mode, we do not actually create the entity.
        }

        return serviceClient.Create(entity);
    }

    public Guid Create(Entity entity, ParameterCollection parameters)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity), "The provided entity cannot be null.");
        }

        if (options.DryRun)
        {
            LogOperation(entity, parameters);
            return Guid.NewGuid(); // In dry run mode, we do not actually create the entity.
        }

        var req = new CreateRequest
        {
            Target = entity
        };
        req.Parameters.AddRange(parameters);

        return serviceClient.Execute(req) is CreateResponse response
            ? response.id
            : throw new InvalidOperationException("Failed to create entity with provided parameters.");
    }

    public void Update(Entity entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity), "The provided entity cannot be null.");
        }

        if (options.DryRun)
        {
            LogOperation(entity);
            return; // In dry run mode, we do not actually update the entity.
        }

        serviceClient.Update(entity);
    }

    public void PerformAsBulkWithOutput<T>(List<T> updates) where T : OrganizationRequest
    {
        var responses = PerformAsBulk(updates);
        var failedReponses = responses.Where(x => x.Fault != null).ToList();
        if (failedReponses.Count > 0)
        {
            logger.LogError($"Error when performing {failedReponses.Count} requests.");
            throw new Exception("PerformAsBulkWithOutput encountered an error in one or more of the requests.");
        }
        else
        {
            logger.LogTrace($"Succesfully performed {updates.Count} actions.");
        }
    }

    public List<ExecuteMultipleResponseItem> PerformAsBulk<T>(List<T> updates) where T : OrganizationRequest
    {
        if (options.DryRun)
        {
            logger.LogDebug("DRY RUN: Would execute {0} {1} requests", updates.Count, typeof(T).Name);
            return []; // In dry run mode, we do not actually perform the requests.
        }

        var chunks = updates.Chunk(200);
        var responses = new List<ExecuteMultipleResponseItem>();
        foreach (var chunk in chunks)
        {
            logger.LogTrace("Executing batch of {length}", chunk.Length);

            var req = new ExecuteMultipleRequest();
            req.Requests = [.. chunk];
            req.Settings = new ExecuteMultipleSettings
            {
                ContinueOnError = true,
                ReturnResponses = true,
            };
            var response = (ExecuteMultipleResponse)serviceClient.Execute(req);
            responses.AddRange([.. response.Responses]);
        }

        return responses;
    }

    private void LogOperation(Entity entity, ParameterCollection? parameters = null, [CallerMemberName] string operation = "")
    {
        logger.LogDebug("DRY RUN: {Operation} operation would be performed for entity of type '{EntityType}'.",
                        operation, entity.LogicalName);

        logger.LogTrace("{attrs}", string.Join("\n", entity.Attributes.Select(kvp => $" - {kvp.Key}: {TruncateValue(kvp.Value)}")));

        if (parameters != null && parameters.Count > 0)
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
