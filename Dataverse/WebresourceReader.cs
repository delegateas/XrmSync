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

	public IEnumerable<WebresourceDependency> GetWebresourcesWithDependencies(IEnumerable<WebresourceDefinition> webresources)
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
		).ConvertAll(dn => new { DependencyNodeId = dn.Id, dn.ObjectId });

		// We have the dependency nodes that map to the webresources,
		// now we can find dependencies that reference these nodes as the requiered object
		var dependencies = reader.RetrieveByColumn<Dependency>(
			d => d.RequiredComponentNodeId,
			dependencyNodes.Select(dn => dn.DependencyNodeId),
			d => d.RequiredComponentObjectId,
			d => d.DependentComponentObjectId
		).ConvertAll(d => new
		{
			RequiredObjectId = d.RequiredComponentObjectId,
			DependentObjectId = d.DependentComponentObjectId
		}).ToLookup(d => d.DependentObjectId, d => d.RequiredObjectId);

		// Get the dependent objects
		var dependentComponents = reader.RetrieveByColumn<SolutionComponent, Guid?>(
			sc => sc.ObjectId,
			dependencies.Select(g => g.Key).Distinct(),
			sc => sc.ComponentType,
			sc => sc.ObjectId
		).ConvertAll(dc => new
		{
			DependentObjectId = dc.ObjectId ?? Guid.Empty,
			dc.ComponentType
		});

		// Map back to WebresourceDependency
		return dependentComponents.SelectMany(dep =>
			webresources
			.Where(w => dependencies[dep.DependentObjectId].Contains(w.Id))
			.Select(dw =>
				new WebresourceDependency(
					dw,
					dep.ComponentType?.ToString() ?? "Unknown",
					dep.DependentObjectId
				)
			)
		);
	}

	public Dictionary<string, Guid> GetWebresourcesByNames(IEnumerable<string> names)
	{
		var namesList = names.ToList();

		if (namesList.Count == 0)
		{
			return [];
		}

		var webresources = reader.RetrieveByColumn<WebResource, string>(
			wr => wr.Name,
			namesList,
			wr => wr.Name
		);

		return webresources
			.Where(wr => wr.Name != null && wr.Id != Guid.Empty)
			.ToDictionary(wr => wr.Name ?? string.Empty, wr => wr.Id, StringComparer.OrdinalIgnoreCase);
	}
}
