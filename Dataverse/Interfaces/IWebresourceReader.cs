using XrmSync.Model.Webresource;

namespace XrmSync.Dataverse.Interfaces;

public interface IWebresourceReader
{
    List<WebresourceDefinition> GetWebresources(Guid solutionId);
}
