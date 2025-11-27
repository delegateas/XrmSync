using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using XrmSync.Options;

namespace XrmSync.Commands;

internal class ConfigListCommand : XrmSyncCommandBase
{
	public ConfigListCommand() : base("list", "List all available named configurations")
	{
		AddSharedOptions();

		SetAction(ExecuteAsync);
	}

	private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
	{
		// Build service provider with just IConfiguration (no need to build full XrmSyncConfiguration)
		var serviceProvider = GetConfigListServices()
			.BuildServiceProvider();

		try
		{
			var output = serviceProvider.GetRequiredService<IConfigValidationOutput>();
			await output.OutputConfigList(cancellationToken);

			return E_OK;
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"Configuration list error:{Environment.NewLine}{ex.Message}");
			return E_ERROR;
		}
	}

	private static IServiceCollection GetConfigListServices(IServiceCollection? services = null)
	{
		services ??= new ServiceCollection();

		// Register IConfigReader to get access to IConfiguration
		services.AddSingleton<IConfigReader, ConfigReader>();
		services.AddSingleton(sp => sp.GetRequiredService<IConfigReader>().GetConfiguration());
		services.AddSingleton<IConfigValidationOutput, ConfigValidationOutput>();

		return services;
	}
}
