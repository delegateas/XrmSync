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

    /// <summary>
    /// Retrieves webresources by their names.
    /// </summary>
    /// <param name="names">The names of the webresources to retrieve.</param>
    /// <returns>A dictionary mapping webresource names to their IDs.</returns>
    Dictionary<string, Guid> GetWebresourcesByNames(IEnumerable<string> names);
}

public record WebresourceDependency(WebresourceDefinition Webresource, string DependentObjectType, Guid DependentObjectId);