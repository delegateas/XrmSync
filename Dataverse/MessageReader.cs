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

    public Dictionary<string, MessageFilterMap> GetMessageFilters(IEnumerable<string> messageNames, IEnumerable<string> entityNames)
    {
        if (!messageNames.Any())
            return [];

        var query = new QueryExpression(SdkMessage.EntityLogicalName)
        {
            ColumnSet = new ColumnSet(SdkMessage.Fields.Name)
        };

        var filter = new FilterExpression();
        filter.AddCondition(SdkMessage.Fields.Name, ConditionOperator.In, [.. messageNames]);
        query.Criteria = filter;

        if (entityNames.Any())
        {
            var messageFilterLink = query.AddLink(SdkMessageFilter.EntityLogicalName, SdkMessage.PrimaryIdAttribute, SdkMessageFilter.Fields.SdkMessageId, JoinOperator.LeftOuter);
            messageFilterLink.Columns = new ColumnSet(
                SdkMessageFilter.PrimaryIdAttribute,
                SdkMessageFilter.Fields.PrimaryObjectTypeCode
            );
            messageFilterLink.EntityAlias = "mf";
            messageFilterLink.LinkCriteria.AddCondition(SdkMessageFilter.Fields.PrimaryObjectTypeCode, ConditionOperator.In, [.. entityNames]);
        }

        return reader.RetrieveMultiple(query)
            .GroupBy(e => e.Id)
            .Select(group =>
            {
                var message = group.First();
                var messageId = message.Id;
                var messageName = message.GetAttributeValue<string>(SdkMessage.Fields.Name);

                // PrimaryObject -> MessageFilterId
                var messageFilterIds = group
                    .Where(g => g.Contains($"mf.{SdkMessageFilter.Fields.PrimaryObjectTypeCode}"))
                    .ToDictionary(
                        g => g.GetAttributeValue<AliasedValue>($"mf.{SdkMessageFilter.Fields.PrimaryObjectTypeCode}")?.Value as string ?? string.Empty,
                        g => g.GetAttributeValue<AliasedValue>($"mf.{SdkMessageFilter.PrimaryIdAttribute}")?.Value as Guid? ?? Guid.Empty
                    );

                return (
                    messageName,
                    messageId,
                    messageFilterIds
                );
            })
            .ToDictionary(
                x => x.messageName,
                x => new MessageFilterMap(x.messageId, x.messageFilterIds)
            );
    }
}
