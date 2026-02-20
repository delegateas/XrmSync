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
		return BuildBaseServices().BuildServiceProvider();
	}

	/// <summary>
	/// Builds a service provider with Dataverse services and PluginSyncCommandOptions registered.
	/// Use this when testing writers that depend on IOptions&lt;PluginSyncCommandOptions&gt;.
	/// </summary>
	protected IServiceProvider BuildPluginServiceProvider(string solutionName)
	{
		return BuildBaseServices()
			.AddSingleton(Options.Create(new PluginSyncCommandOptions("test.dll", solutionName)))
			.BuildServiceProvider();
	}

	/// <summary>
	/// Builds a service provider with Dataverse services and WebresourceSyncCommandOptions registered.
	/// Use this when testing writers that depend on IOptions&lt;WebresourceSyncCommandOptions&gt;.
	/// </summary>
	protected IServiceProvider BuildWebresourceServiceProvider(string solutionName)
	{
		return BuildBaseServices()
			.AddSingleton(Options.Create(new WebresourceSyncCommandOptions("test/", solutionName)))
			.BuildServiceProvider();
	}

	/// <summary>
	/// Creates a base service collection with all common Dataverse services registered.
	/// </summary>
	private IServiceCollection BuildBaseServices()
	{
		var services = new ServiceCollection();

		// Register test service provider (XrmMockup's IOrganizationService)
		services.AddSingleton(ServiceProvider);

		// Configure execution options (not dry run for tests)
		services.AddSingleton(Options.Create(new ExecutionModeOptions(DryRun: false)));

		// Reuse all reader/writer registrations from production code
		services.AddDataverseServices();

		services.AddLogging();

		return services;
	}

	/// <summary>
	/// Creates a mock logger for the specified type.
	/// </summary>
	protected static ILogger<T> CreateLogger<T>() => Substitute.For<ILogger<T>>();
}
