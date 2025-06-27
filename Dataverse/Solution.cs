using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DG.XrmPluginSync.Dataverse;

public class Solution(CrmDataHelper crmDataHelper)
{
    public Guid GetSolutionId(string solutionName) => RetrieveSolution(solutionName, new ColumnSet(null)).Id;

    public Entity RetrieveSolution(string uniqueName, ColumnSet columnSet)
    {
        FilterExpression f = new();
        f.AddCondition(new ConditionExpression("uniquename", ConditionOperator.Equal, uniqueName));

        QueryExpression q = new("solution")
        {
            ColumnSet = columnSet,
            Criteria = f
        };

        return crmDataHelper.RetrieveFirstMatch(q);
    }
}
