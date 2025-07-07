using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DG.XrmSync.Dataverse.Interfaces;

public interface IDataverseReader
{
    Entity Retrieve(string logicalName, Guid id, ColumnSet columnSet);
    Entity RetrieveFirstMatch(QueryExpression query);
    bool Exists(string logicalName, Guid id);
    Entity? RetrieveFirstOrDefault(QueryExpression query);
    List<Entity> RetrieveMultiple(QueryExpression queryExpression);
}
