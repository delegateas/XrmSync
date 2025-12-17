using Microsoft.Xrm.Sdk;
using XrmSync.Dataverse.Interfaces;

namespace Tests.Integration.Infrastructure;

/// <summary>
/// Mock implementation of IOrganizationServiceProvider for XrmMockup testing.
/// </summary>
public sealed class MockOrganizationServiceProvider(IOrganizationService service) : IOrganizationServiceProvider
{
	public IOrganizationService Service { get; } = service;

	public string ConnectedHost => "https://test.crm.dynamics.com";
}
