using DG.XrmPluginSync.Dataverse.Interfaces;
using DG.XrmPluginSync.Model;
using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace DG.XrmPluginSync.Dataverse;

public sealed class DataverseWriter : IDataverseWriter
{
    private readonly ServiceClient serviceClient;
    private readonly ILogger logger;

    public DataverseWriter(ServiceClient serviceClient, ILogger logger, XrmPluginSyncOptions options)
    {
        if (options.DryRun)
        {
            throw new InvalidOperationException("Cannot perform write operations in dry run mode. Please disable dry run to proceed with writing to Dataverse.");
        }

        this.serviceClient = serviceClient;
        this.logger = logger;
    }

    public Guid Create(Entity entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity), "The provided entity cannot be null.");
        }

        return serviceClient.Create(entity);
    }

    public Guid Create(Entity entity, ParameterCollection parameters)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity), "The provided entity cannot be null.");
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
}
