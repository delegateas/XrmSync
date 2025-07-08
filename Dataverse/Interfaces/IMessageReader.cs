namespace XrmSync.Dataverse.Interfaces;

public interface IMessageReader
{
    Dictionary<string, MessageFilterMap> GetMessageFilters(IEnumerable<string> names, IEnumerable<string> entityNames);
}

public record MessageFilterMap(Guid MessageId, Dictionary<string, Guid> FilterMap);