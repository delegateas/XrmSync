using DG.XrmPluginSync.Model;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;

namespace DG.XrmPluginSync.Dataverse.Extensions;

public static class EntityExtensions
{
    public static IEnumerable<DeleteRequest> ToDeleteRequests<T>(this IEnumerable<T> entities, string entityTypeName) where T : EntityBase
    {
        return entities.Select(x => ToDeleteRequest(x, entityTypeName));
    }

    public static DeleteRequest ToDeleteRequest<T>(this T entity, string entityTypeName) where T : EntityBase
    {
        return new DeleteRequest
        {
            Target = new EntityReference(entityTypeName, entity.Id)
        };
    }
}
