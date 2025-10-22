using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.Webresource;

namespace XrmSync.Dataverse;

internal class WebresourceReader(IDataverseReader reader) : IWebresourceReader
{
    public List<WebresourceDefinition> GetWebresources(Guid solutionId)
    {
        // Yes, the join operation is heavy,
        // Yes, we use RetrieveByColumn in the other getters.
        // However, this call is limited by having to fetch the contents of the webresources, so the join is neglible in comparison.
        return [.. (
            from wr in reader.WebResources
            join sc in reader.SolutionComponents on wr.Id equals sc.ObjectId
            where
                sc.SolutionId != null && sc.SolutionId.Id == solutionId
                && wr.IsManaged != true
            orderby wr.Name
            select new
            {
                wr.Id,
                wr.Name,
                wr.DisplayName,
                wr.WebResourceType,
                wr.Content
            }
        ).AsEnumerable()
        .Select(wr => new WebresourceDefinition(
            wr.Name ?? string.Empty,
            wr.DisplayName ?? string.Empty,
            (WebresourceType)(wr.WebResourceType ?? 0),
            wr.Content ?? string.Empty
        )
        {
            Id = wr.Id
        })];
    }

    public List<WebresourceDefinition> GetWebresourcesWithDependencies(IEnumerable<WebresourceDefinition> webresources)
    {
        var webresourceIds = webresources.Select(w => w.Id).ToList();

        if (webresourceIds.Count == 0)
        {
            return [];
        }

        // Find dependency nodes for the webresources
        var dependencyNodes = reader.RetrieveByColumn<DependencyNode>(
            dn => dn.ObjectId,
            webresources.Select(w => w.Id),
            dn => dn.DependencyNodeId,
            dn => dn.ObjectId
        ).ConvertAll(dn => new { dn.Id, dn.ObjectId });

        // We have the dependency nodes that map to the webresources,
        // now we can find dependencies that reference these nodes as the requiered object
        var requiredIds = reader.RetrieveByColumn<Dependency>(
            d => d.RequiredComponentObjectId,
            dependencyNodes.Select(dn => dn.Id),
            d => d.RequiredComponentObjectId
        ).Select(d => d.RequiredComponentObjectId).Distinct().ToList();

        // We have a list of required component node ids, match them back to webresources
        return [.. (
            from dn in dependencyNodes
            where requiredIds.Contains(dn.Id)
            join w in webresources on dn.ObjectId equals w.Id
            select w
        ).Distinct()];
    }
}
