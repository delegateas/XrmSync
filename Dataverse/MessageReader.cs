using DG.XrmPluginSync.Dataverse.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DG.XrmPluginSync.Dataverse;

public class MessageReader(IDataverseReader reader) : IMessageReader
{
    public static string? GetMessagePropertyName(string eventOperation) => eventOperation switch
    {
        "Assign" => "Target",
        "Create" => "id",
        "Delete" => "Target",
        "DeliverIncoming" => "emailid",
        "DeliverPromote" => "emailid",
        "Merge" => "Target",
        "Route" => "Target",
        "Send" => "emailid",
        "SetState" => "entityMoniker",
        "SetStateDynamicEntity" => "entityMoniker",
        "Update" => "Target",
        _ => null,
    };

    public Dictionary<string, Guid> GetMessages(IEnumerable<string> names)
    {
        var query = new QueryExpression(EntityTypeNames.Message)
        {
            ColumnSet = new ColumnSet("sdkmessageid", "name")
        };

        var filter = new FilterExpression();
        filter.AddCondition(new ConditionExpression("name", ConditionOperator.In, [.. names]));
        query.Criteria = filter;

        return reader.RetrieveMultiple(query)
            .ToDictionary(e => e.GetAttributeValue<string>("name"), e => e.GetAttributeValue<Guid>("sdkmessageid"));
    }

    public Entity GetMessageFilter(string primaryObjectType, Guid sdkMessageId)
    {
        var query = new QueryExpression(EntityTypeNames.MessageFilter)
        {
            ColumnSet = new ColumnSet("sdkmessagefilterid")
        };

        var filter = new FilterExpression();
        filter.AddCondition(new ConditionExpression("sdkmessageid", ConditionOperator.Equal, sdkMessageId));
        query.Criteria = filter;

        if (!string.IsNullOrEmpty(primaryObjectType))
            filter.AddCondition(new ConditionExpression("primaryobjecttypecode", ConditionOperator.Equal, primaryObjectType));

        return reader.RetrieveFirstOrDefault(query);
    }
}
