using Microsoft.Xrm.Sdk.Query;
using Tests.Integration.Infrastructure;

namespace Tests.Integration;

/// <summary>
/// Smoke tests to verify XrmMockup integration works correctly.
/// </summary>
public sealed class SmokeTests : TestBase
{
	[Fact]
	public void XrmMockup_CanCreateAndRetrieveSolution()
	{
		// Arrange
		var (solutionId, _) = Producer.ProduceSolution("test_smoketest");

		// Act
		var retrieved = Service.Retrieve("solution", solutionId, new ColumnSet("uniquename"));

		// Assert
		Assert.Equal("test_smoketest", retrieved.GetAttributeValue<string>("uniquename"));
	}

	[Fact]
	public void XrmMockup_CanCreatePluginAssembly()
	{
		// Arrange
		var assemblyId = Producer.ProducePluginAssembly("TestAssembly", "1.0.0.0");

		// Act
		var retrieved = Service.Retrieve("pluginassembly", assemblyId, new ColumnSet("name", "version"));

		// Assert
		Assert.Equal("TestAssembly", retrieved.GetAttributeValue<string>("name"));
		Assert.Equal("1.0.0.0", retrieved.GetAttributeValue<string>("version"));
	}

	[Fact]
	public void ServiceProvider_ReturnsValidService()
	{
		// Assert
		Assert.NotNull(ServiceProvider.Service);
		Assert.Equal("https://test.crm.dynamics.com", ServiceProvider.ConnectedHost);
	}

	[Fact]
	public void BuildServiceProvider_CreatesValidDIContainer()
	{
		// Act
		var sp = BuildServiceProvider();

		// Assert
		Assert.NotNull(sp);
	}
}
