using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace DG.XrmPluginSync.Dataverse.Interfaces;

public interface ISolutionReader
{
    Guid GetSolutionId(string solutionName);
    Entity RetrieveSolution(string uniqueName, ColumnSet columnSet);
}