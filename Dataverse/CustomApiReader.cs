using DG.XrmPluginSync.Dataverse.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DG.XrmPluginSync.Dataverse;

public class CustomApiReader(IDataverseReader reader) : ICustomApiReader
{
    public List<Entity> GetCustomApis(Guid solutionId)
    {
        var link = new LinkEntity
        {
            JoinOperator = JoinOperator.Inner,
            LinkFromAttributeName = "customapiid",
            LinkFromEntityName = "customapi",
            LinkToAttributeName = "objectid",
            LinkToEntityName = "solutioncomponent"
        };
        link.LinkCriteria.Conditions.Add(new ConditionExpression("solutionid", ConditionOperator.Equal, solutionId));

        var query = new QueryExpression(EntityTypeNames.CustomApi)
        {
            ColumnSet = new ColumnSet(true)
        };
        query.LinkEntities.Add(link);

        return reader.RetrieveMultiple(query);
    }

    public List<Entity> GetCustomApiRequestParameters(Guid customApiId)
    {
        var filter = new FilterExpression();
        filter.AddCondition(new ConditionExpression("customapiid", ConditionOperator.Equal, customApiId));
        var query = new QueryExpression(EntityTypeNames.RequestParameter)
        {
            ColumnSet = new ColumnSet(true),
            Criteria = filter
        };
        return reader.RetrieveMultiple(query);
    }

    public List<Entity> GetCustomApiResponseProperties(Guid customApiId)
    {
        var filter = new FilterExpression();
        filter.AddCondition(new ConditionExpression("customapiid", ConditionOperator.Equal, customApiId));
        var query = new QueryExpression(EntityTypeNames.ResponseProperty)
        {
            ColumnSet = new ColumnSet(true),
            Criteria = filter
        };
        return reader.RetrieveMultiple(query);
    }
}
