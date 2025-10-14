using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Interfaces;

namespace XrmSync.Dataverse;

internal class MessageReader(IDataverseReader reader) : IMessageReader
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

        // Get relevant messages
        var messages = reader.RetrieveByColumn<SdkMessage, string>(
            m => m.Name,
            messageNames,
            m => m.Name
        ).ConvertAll(m => new
        {
            m.Id,
            m.Name
        });

        // Get relevant message filters for the selected entities
        var messageFilters = reader.RetrieveByColumn<SdkMessageFilter, string>(
            mf => mf.PrimaryObjectTypeCode,
            entityNames,
            mf => mf.PrimaryObjectTypeCode,
            mf => mf.SdkMessageId
        ).ToLookup(mf => mf.SdkMessageId?.Id ?? Guid.Empty, mf => new
        {
            mf.Id,
            mf.PrimaryObjectTypeCode
        });

        return messages.Select(message => (
            MessageName: message.Name ?? string.Empty,
            MessageId: message.Id,
            MessageFilters: messageFilters[message.Id].ToDictionary(
                mf => mf.PrimaryObjectTypeCode ?? string.Empty,
                mf => mf.Id
            )
        )).ToDictionary(
            x => x.MessageName,
            x => new MessageFilterMap(x.MessageId, x.MessageFilters)
        );
    }
}
