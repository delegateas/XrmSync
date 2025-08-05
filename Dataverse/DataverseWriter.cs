using Microsoft.Extensions.Logging;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Exceptions;

namespace XrmSync.Dataverse;

public sealed class DataverseWriter : IDataverseWriter
{
    private readonly ServiceClient serviceClient;
    private readonly ILogger logger;

    public DataverseWriter(ServiceClient serviceClient, ILogger logger, XrmSyncOptions options)
    {
        if (options.DryRun)
        {
            throw new XrmSyncException("Cannot perform write operations in dry run mode. Please disable dry run to proceed with writing to Dataverse.");
        }

        this.serviceClient = serviceClient;
        this.logger = logger;
    }

    public Guid Create(Entity entity, IDictionary<string, object>? parameters = null)
    {
        if (entity == null)
        {
            throw new XrmSyncException("The provided entity cannot be null.");
        }

        if (parameters == null)
        {
            return serviceClient.Create(entity);
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
            throw new XrmSyncException("The provided entity cannot be null.");
        }

        serviceClient.Update(entity);
    }

    public void UpdateMultiple<TEntity>(List<TEntity> entities) where TEntity : Entity
    {
        PerformAsBulk(entities.ConvertAll(e => new UpdateRequest { Target = e }));
    }

    public void PerformAsBulk<T>(List<T> updates) where T : OrganizationRequest
    {
        var responses = PerformAsBulkInner(updates);

        var failedReponses = responses.Where(x => x.Fault != null).ToList();
        if (failedReponses.Count > 0)
        {
            logger.LogError("Error when performing {count} requests.", failedReponses.Count);
            failedReponses.ForEach(f =>
            {
            var update = updates[f.RequestIndex];

                var (entityName, entityId) = update switch
                {
                    CreateRequest cr => (cr.Target.LogicalName, cr.Target.Id),
                    UpdateRequest ur => (ur.Target.LogicalName, ur.Target.Id),
                    DeleteRequest dr => (dr.Target.LogicalName, dr.Target.Id),
                    _ => throw new XrmSyncException($"Unexpected request type: {typeof(T)}, expected Create, Update or Delete request")
                };

                var prefix = $" - {update.RequestName} for {entityName} with ID {entityId}: ";
                if (f.Fault.InnerFault is null || f.Fault.Message.Equals(f.Fault.InnerFault.Message, StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogError("{prefix}{message}", prefix, f.Fault.Message);
                }
                else
                {
                    logger.LogError("{prefix}{message}: {innerFault}", prefix, f.Fault.Message, f.Fault.InnerFault.Message);
                }

                if (!string.IsNullOrEmpty(f.Fault.TraceText))
                    logger.LogTrace("   {trace}", f.Fault.TraceText);
            });
            throw new XrmSyncException("PerformAsBulkWithOutput encountered an error in one or more of the requests.");
        }
        else
        {
            logger.LogTrace("Succesfully performed {count} actions.", updates.Count);
        }
    }

    private List<ExecuteMultipleResponseItem> PerformAsBulkInner<T>(List<T> updates) where T : OrganizationRequest
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
