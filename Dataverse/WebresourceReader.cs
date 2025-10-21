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
}
