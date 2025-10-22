using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using System.Linq.Expressions;
using System.Reflection;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Model.Extensions;

namespace XrmSync.Dataverse.Extensions;

public static class EntityExtensions
{
    public static IEnumerable<DeleteRequest> ToDeleteRequests<TEntity>(this IEnumerable<TEntity> entities, string entityTypeName) where TEntity : EntityBase
        => ToDeleteRequests(entities.Select(e => (e.Id, entityTypeName)));

    public static IEnumerable<DeleteRequest> ToDeleteRequests<TEntity>(this IEnumerable<TEntity> entities) where TEntity : Entity
        => ToDeleteRequests(entities.Select(e => (e.Id, e.LogicalName)));

    public static IEnumerable<DeleteRequest> ToDeleteRequests(this IEnumerable<(Guid Id, string EntityLogicalName)> entities)
    {
        return entities.Select(entity => new DeleteRequest
        {
            Target = new EntityReference(entity.EntityLogicalName, entity.Id)
        });
    }

    public static string GetColumnName<TEntity>(this Expression<Func<TEntity, object?>> lambda) where TEntity : Entity
    {
        var member = lambda.GetMemberInfo();

        var attributeLogicalNameAttribute =
            member.GetCustomAttribute<AttributeLogicalNameAttribute>()
            ?? throw new XrmSyncException($"Member '{member.Name}' does not have an AttributeLogicalName attribute");

        return attributeLogicalNameAttribute.LogicalName;
    }

    public static string GetColumnName<T, TValue>(this Expression<Func<T, TValue?>> lambda) where T : Entity
    {
        var member = lambda.GetMemberInfo();
        var attributeLogicalNameAttribute =
            member.GetCustomAttribute<AttributeLogicalNameAttribute>()
            ?? throw new XrmSyncException($"Member '{member.Name}' does not have an AttributeLogicalName attribute");
        
        return attributeLogicalNameAttribute.LogicalName;
    }
}
