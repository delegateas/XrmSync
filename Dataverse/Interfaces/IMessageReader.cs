namespace XrmSync.Dataverse.Interfaces;

public interface IMessageReader
{
    Guid? GetMessageFilterId(string primaryObjectType, Guid sdkMessageId);
    Dictionary<string, Guid> GetMessages(IEnumerable<string> names);
}