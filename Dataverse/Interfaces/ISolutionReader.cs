namespace XrmSync.Dataverse.Interfaces;

public interface ISolutionReader
{
    string ConnectedHost { get; }
    (Guid SolutionId, string Prefix) RetrieveSolution(string uniqueName);
}