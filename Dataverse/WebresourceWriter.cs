using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XrmSync.Dataverse.Context;
using XrmSync.Dataverse.Extensions;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;
using XrmSync.Model.Webresource;

namespace XrmSync.Dataverse;

internal class WebresourceWriter(IDataverseWriter writer, IOptions<WebresourceSyncOptions> configuration) : IWebresourceWriter
{
    private Dictionary<string, object> Parameters { get; } = new() {
            { "SolutionUniqueName", configuration.Value.SolutionName }
    };

    public void Create(IEnumerable<WebresourceDefinition> webresources)
    {
        foreach (var wr in webresources)
        {
            writer.Create(new WebResource
            {
                Name = wr.Name,
                Content = wr.Content,
                DisplayName = wr.DisplayName,
                WebResourceType = (WebResource_WebResourceType)wr.Type
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
}
