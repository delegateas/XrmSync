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
    public static IEnumerable<DeleteRequest> ToDeleteRequests<T>(this IEnumerable<T> entities, string entityTypeName) where T : EntityBase
    {
        return entities.Select(x => x.ToDeleteRequest(entityTypeName));
    }

    public static DeleteRequest ToDeleteRequest<T>(this T entity, string entityTypeName) where T : EntityBase
    {
        return new DeleteRequest
        {
            Target = new EntityReference(entityTypeName, entity.Id)
        };
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
