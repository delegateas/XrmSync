using Microsoft.PowerPlatform.Dataverse.Client;
using Microsoft.Xrm.Sdk;
using XrmSync.Dataverse.Interfaces;

namespace XrmSync.Dataverse;

/// <summary>
/// Production implementation of IOrganizationServiceProvider using ServiceClient.
/// </summary>
internal sealed class ServiceClientProvider(ServiceClient serviceClient) : IOrganizationServiceProvider
{
	public IOrganizationService Service => serviceClient;

	public string ConnectedHost => serviceClient.ConnectedOrgUriActual.GetLeftPart(UriPartial.Authority);
}
