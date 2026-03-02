using Microsoft.Xrm.Sdk.Query;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Extensions;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model.Webresource;
namespace XrmSync.Dataverse;

internal class WebresourceReader(IDataverseReader reader, IOrganizationServiceProvider serviceProvider) : IWebresourceReader
{
	public List<WebresourceDefinition> GetWebresources(Guid solutionId, IEnumerable<WebresourceType>? allowedTypes = null)
	{
		var query = new QueryExpression(WebResource.EntityLogicalName)
		{
			ColumnSet = new ColumnSet(
				WebResource.ColumnName(x => x.Name),
				WebResource.ColumnName(x => x.DisplayName),
				WebResource.ColumnName(x => x.WebResourceType),
				WebResource.ColumnName(x => x.Content)),
			Orders = { new OrderExpression(WebResource.ColumnName(x => x.Name), OrderType.Ascending) }
		};

		// Join to solutioncomponent to filter by solution
		var solutionComponentLink = query.AddLink(
			SolutionComponent.EntityLogicalName,
			WebResource.ColumnName(x => x.WebResourceId),
			SolutionComponent.ColumnName(x => x.ObjectId));

		solutionComponentLink.LinkCriteria.AddCondition(
			SolutionComponent.ColumnName(x => x.SolutionId), ConditionOperator.Equal, solutionId);

		// Filter out managed webresources
		query.Criteria.AddCondition(
			WebResource.ColumnName(x => x.IsManaged), ConditionOperator.NotEqual, true);

		// Filter by allowed types if specified
		var typesList = allowedTypes?.ToList();
		if (typesList is { Count: > 0 })
		{
			query.Criteria.AddCondition(
				WebResource.ColumnName(x => x.WebResourceType), ConditionOperator.In,
				[.. typesList.Select(t => (object)(int)t)]);
		}

		var result = serviceProvider.Service.RetrieveMultiple(query);

		return [.. result.Entities
			.Select(e => e.ToEntity<WebResource>())
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
