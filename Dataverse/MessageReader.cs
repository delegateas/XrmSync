using Microsoft.Xrm.Sdk.Query;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Interfaces;

namespace XrmSync.Dataverse;

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
        if (!names.Any())
            return [];

        var query = new QueryExpression(SdkMessage.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(SdkMessage.Fields.Name)
        };

        var filter = new FilterExpression();
        filter.AddCondition(SdkMessage.Fields.Name, ConditionOperator.In, [.. names]);
        query.Criteria = filter;

        return reader.RetrieveMultiple(query)
            .ToDictionary(e => e.GetAttributeValue<string>(SdkMessage.Fields.Name), e => e.Id);
    }

    public Guid? GetMessageFilterId(string primaryObjectType, Guid sdkMessageId)
    {
        var query = new QueryExpression(SdkMessageFilter.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(SdkMessageFilter.PrimaryIdAttribute)
        };

        var filter = new FilterExpression();
        filter.AddCondition(SdkMessageFilter.Fields.SdkMessageId, ConditionOperator.Equal, sdkMessageId);
        query.Criteria = filter;

        if (!string.IsNullOrEmpty(primaryObjectType))
            filter.AddCondition(SdkMessageFilter.Fields.PrimaryObjectTypeCode, ConditionOperator.Equal, primaryObjectType);

        var messageFilter = reader.RetrieveFirstOrDefault(query);
        return messageFilter?.Id;
    }
}
