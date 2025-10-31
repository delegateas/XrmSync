using XrmSync.Model.Webresource;

namespace XrmSync.Dataverse.Interfaces;

public interface IWebresourceReader
{
    List<WebresourceDefinition> GetWebresources(Guid solutionId);

    /// <summary>
    /// Gets the webresources that have dependencies (i.e., objects that depend on them).
    /// </summary>
    /// <param name="webresources">The webresources to check for dependencies.</param>
    /// <returns>A list of webresources that have dependencies.</returns>
    IEnumerable<WebresourceDependency> GetWebresourcesWithDependencies(IEnumerable<WebresourceDefinition> webresources);
}

public record WebresourceDependency(WebresourceDefinition Webresource, string DependentObjectType, Guid DependentObjectId);