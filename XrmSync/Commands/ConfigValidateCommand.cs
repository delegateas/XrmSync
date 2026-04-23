using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;
using XrmSync.Constants;
using XrmSync.Extensions;
using XrmSync.Options;

namespace XrmSync.Commands;

internal class ConfigValidateCommand : XrmSyncCommandBase
{
	private static readonly Option<bool> AllOption = CliOptions.Config.All.CreateOption<bool>();

	public ConfigValidateCommand() : base("validate", "Validate configuration from appsettings.json")
	{
		AddSharedOptions();
		Add(AllOption);

		SetAction(ExecuteAsync);
	}

	private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
	{
		var all = parseResult.GetValue(AllOption);

		// Build service provider - no overrides, just validate what's in the config
		var serviceProvider = GetConfigValidateServices()
			.AddXrmSyncConfiguration(new ExecutionContext(null, null, null, null, parseResult.GetValue(CommandOptions.Profile)))
			.AddOptions(baseOptions => baseOptions) // No overrides
			.BuildServiceProvider();

		try
		{
			var output = serviceProvider.GetRequiredService<IConfigValidationOutput>();

			if (all)
				await output.OutputAllValidationResults(cancellationToken);
			else
				await output.OutputValidationResult(cancellationToken);

			return E_OK;
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine($"Configuration validation error:{Environment.NewLine}{ex.Message}");
			return E_ERROR;
		}
	}

	private static IServiceCollection GetConfigValidateServices(IServiceCollection? services = null)
	{
		services ??= new ServiceCollection();

		services.AddSingleton<IConfigValidationOutput, ConfigValidationOutput>();

		return services;
	}
}
