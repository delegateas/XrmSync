using DG.XrmPluginSync.Dataverse.Interfaces;
using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DG.XrmPluginSync.Dataverse;

public class SolutionReader(ServiceClient serviceClient) : DataverseReader(serviceClient), ISolutionReader
{
    public const string EntityTypeName = "solution";

    public Guid GetSolutionId(string solutionName) => RetrieveSolution(solutionName, new ColumnSet(null)).Id;

    public Entity RetrieveSolution(string uniqueName, ColumnSet columnSet)
    {
        FilterExpression f = new();
        f.AddCondition(new ConditionExpression("uniquename", ConditionOperator.Equal, uniqueName));

        QueryExpression q = new(EntityTypeName)
        {
            ColumnSet = columnSet,
            Criteria = f
        };

        return RetrieveFirstMatch(q);
    }
}
