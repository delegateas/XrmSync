using Microsoft.Xrm.Sdk;

namespace XrmSync.Dataverse.Interfaces;

public interface IMessageReader
{
    Entity? GetMessageFilter(string primaryObjectType, Guid sdkMessageId);
    Dictionary<string, Guid> GetMessages(IEnumerable<string> names);
}