using XrmSync.Model.Webresource;

namespace XrmSync.Dataverse.Interfaces;

public interface IWebresourceWriter
{
	void Create(IEnumerable<WebresourceDefinition> webresources);

	void Update(IEnumerable<WebresourceDefinition> webresources);

	void Delete(IEnumerable<WebresourceDefinition> webresources);

	/// <summary>
	/// Publishes the given webresources so the changes take effect immediately.
	/// No-ops when there is nothing to publish.
	/// </summary>
	void Publish(IEnumerable<WebresourceDefinition> webresources);
}
