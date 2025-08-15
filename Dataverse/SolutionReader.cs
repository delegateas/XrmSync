using Microsoft.PowerPlatform.Dataverse.Client;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.Exceptions;

namespace XrmSync.Dataverse;

public class SolutionReader(ServiceClient serviceClient) : ISolutionReader
{
    private readonly Lazy<string> _lazyConnectedHost = new(serviceClient?.ConnectedOrgUriActual?.GetLeftPart(UriPartial.Authority) ?? throw new ArgumentNullException(nameof(serviceClient), "ServiceClient is not initialized"));
    public string ConnectedHost => _lazyConnectedHost.Value;

    public (Guid SolutionId, string Prefix) RetrieveSolution(string uniqueName)
    {
        using var xrm = new DataverseContext(serviceClient);

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var solution = (
            from s in xrm.SolutionSet
            join p in xrm.PublisherSet on s.PublisherId.Id equals p.PublisherId
            where s.UniqueName == uniqueName
            select new
            {
                s.Id,
                p.CustomizationPrefix
            }).FirstOrDefault()
            ?? throw new XrmSyncException($"No solution with unique name {uniqueName} found");
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        return (
            solution.Id,
            solution.CustomizationPrefix
        );
    }
}
