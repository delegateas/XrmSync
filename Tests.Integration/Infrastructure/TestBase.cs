using DG.Tools.XrmMockup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xrm.Sdk;
using NSubstitute;
using XrmSync.Dataverse.Extensions;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;

namespace Tests.Integration.Infrastructure;

/// <summary>
/// Base class for all integration tests using XrmMockup.
/// Each test class gets a fresh XrmMockup instance via parameterless constructor.
/// </summary>
public abstract class TestBase
{
	protected XrmMockup365 Crm { get; }
	protected IOrganizationService Service { get; }
	protected IOrganizationServiceProvider ServiceProvider { get; }
	protected TestDataProducer Producer { get; }

	protected TestBase()
	{
		Crm = XrmMockupFactory.CreateMockup();
		Service = Crm.GetAdminService();
		ServiceProvider = new MockOrganizationServiceProvider(Service);
		Producer = new TestDataProducer(Service);
	}

	/// <summary>
	/// Builds a service provider with all Dataverse services registered.
	/// </summary>
	protected IServiceProvider BuildServiceProvider()
	{
		var services = new ServiceCollection();

		// Register test service provider (XrmMockup's IOrganizationService)
		services.AddSingleton(ServiceProvider);

		// Configure execution options (not dry run for tests)
		services.AddSingleton(Options.Create(new ExecutionModeOptions(DryRun: false)));

		// Reuse all reader/writer registrations from production code
		services.AddDataverseServices();

		services.AddLogging();

		return services.BuildServiceProvider();
	}

	/// <summary>
	/// Creates a mock logger for the specified type.
	/// </summary>
	protected static ILogger<T> CreateLogger<T>() => Substitute.For<ILogger<T>>();
}
