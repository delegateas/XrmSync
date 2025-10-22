using XrmSync.Model.Webresource;

namespace XrmSync.Dataverse.Interfaces;

public interface IWebresourceWriter
{
    void Create(IEnumerable<WebresourceDefinition> webresources);

    void Update(IEnumerable<WebresourceDefinition> webresources);

    void Delete(IEnumerable<WebresourceDefinition> webresources);
}
