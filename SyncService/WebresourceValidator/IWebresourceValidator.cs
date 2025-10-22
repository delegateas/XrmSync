using XrmSync.Model.Webresource;

namespace XrmSync.SyncService.WebresourceValidator;

public interface IWebresourceValidator
{
    void Validate(List<WebresourceDefinition> webresources);
}
