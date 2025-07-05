using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DG.XrmSync.Dataverse.Interfaces;

public interface ISolutionReader
{
    Guid GetSolutionId(string solutionName);
    Entity RetrieveSolution(string uniqueName, ColumnSet columnSet);
}