using DG.XrmSync.Dataverse.Interfaces;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DG.XrmSync.Dataverse;

public class SolutionReader(IDataverseReader reader) : ISolutionReader
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

        return reader.RetrieveFirstMatch(q);
    }
}
