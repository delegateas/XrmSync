using Microsoft.Xrm.Sdk;

namespace XrmSync.Dataverse.Interfaces;

/// <summary>
/// Abstraction over organization service providing access to IOrganizationService and connection info.
/// Enables testing with XrmMockup while using ServiceClient in production.
/// </summary>
public interface IOrganizationServiceProvider
{
	/// <summary>
	/// Gets the organization service for Dataverse operations.
	/// </summary>
	IOrganizationService Service { get; }

	/// <summary>
	/// Gets the connected host URL (e.g., "https://org.crm.dynamics.com").
	/// </summary>
	string ConnectedHost { get; }
}
