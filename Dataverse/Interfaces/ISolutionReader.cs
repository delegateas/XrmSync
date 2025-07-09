namespace XrmSync.Dataverse.Interfaces;

public interface ISolutionReader
{
    (Guid SolutionId, string Prefix) RetrieveSolution(string uniqueName);
}