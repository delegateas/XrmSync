using DataverseConnection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using XrmSync.Dataverse.Interfaces;
using XrmSync.Model;

namespace XrmSync.Dataverse.Extensions;

public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers all Dataverse services (readers, writers).
	/// Requires <see cref="IOrganizationServiceProvider"/> to be registered first.
	/// </summary>
	public static IServiceCollection AddDataverseServices(this IServiceCollection services)
	{
		services.AddSingleton<IDataverseReader, DataverseReader>();
		services.AddSingleton<IDataverseWriter>((sp) =>
		{
			var options = sp.GetRequiredService<IOptions<ExecutionModeOptions>>();

			return options.Value.DryRun
				? ActivatorUtilities.CreateInstance<DryRunDataverseWriter>(sp)
				: ActivatorUtilities.CreateInstance<DataverseWriter>(sp);
		});

		services.AddSingleton<IMessageReader, MessageReader>();
		services.AddSingleton<ISolutionReader, SolutionReader>();

		services.AddSingleton<IPluginAssemblyReader, PluginAssemblyReader>();
		services.AddSingleton<IPluginAssemblyWriter, PluginAssemblyWriter>();

		services.AddSingleton<IPluginReader, PluginReader>();
		services.AddSingleton<IPluginWriter, PluginWriter>();

		services.AddSingleton<ICustomApiReader, CustomApiReader>();
		services.AddSingleton<ICustomApiWriter, CustomApiWriter>();

		services.AddSingleton<IWebresourceReader, WebresourceReader>();
		services.AddSingleton<IWebresourceWriter, WebresourceWriter>();

		return services;
	}

	/// <summary>
	/// Registers Dataverse connection (ServiceClient) and all services.
	/// For production use.
	/// </summary>
	public static IServiceCollection AddDataverseConnection(this IServiceCollection services)
	{
		services.AddDataverse();
		services.AddSingleton<IOrganizationServiceProvider, ServiceClientProvider>();
		return services.AddDataverseServices();
	}
}
