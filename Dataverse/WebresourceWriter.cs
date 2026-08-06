using Microsoft.Extensions.Options;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Extensions;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Webresource;

namespace XrmSync.Dataverse;

internal class WebresourceWriter(IDataverseWriter writer, IOptions<WebresourceSyncCommandOptions> configuration) : IWebresourceWriter
{
	private Dictionary<string, object> Parameters { get; } = new() {
			{ "SolutionUniqueName", configuration.Value.SolutionName }
	};

	public void Create(IEnumerable<WebresourceDefinition> webresources)
	{
		foreach (var wr in webresources)
		{
			// The Id is written back so a follow-up publish can address the newly created records
			wr.Id = writer.Create(new WebResource
			{
				Name = wr.Name,
				Content = wr.Content,
				DisplayName = wr.DisplayName,
				WebResourceType = (webresource_webresourcetype)wr.Type
			}, Parameters);
		}
	}

	public void Update(IEnumerable<WebresourceDefinition> webresources)
	{
		writer.UpdateMultiple(webresources.Select(wr => new WebResource
		{
			Id = wr.Id,
			Content = wr.Content,
			DisplayName = wr.DisplayName
		}));
	}

	public void Delete(IEnumerable<WebresourceDefinition> webresources)
	{
		writer.DeleteMultiple(webresources.ToDeleteRequests(WebResource.EntityLogicalName));
	}

	public void Publish(IEnumerable<WebresourceDefinition> webresources)
	{
		var ids = webresources.Select(wr => wr.Id).Where(id => id != Guid.Empty).Distinct().ToList();
		if (ids.Count == 0)
		{
			// Never issue an empty PublishXmlRequest
			return;
		}

		var elements = string.Concat(ids.Select(id => $"<webresource>{id:B}</webresource>"));
		writer.PublishXml($"<importexportxml><webresources>{elements}</webresources></importexportxml>");
	}
}
