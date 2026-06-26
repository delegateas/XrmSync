using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using XrmSync.Logging;
using XrmSync.Model;
using XrmSync.Options;

using MSOptions = Microsoft.Extensions.Options.Options;

namespace XrmSync.Extensions;

internal static class ServiceCollectionExtensions
{
	public static IServiceCollection AddXrmSyncConfiguration(this IServiceCollection services, ExecutionContext executionContext)
	{
		services
			.AddSingleton<IConfigReader, ConfigReader>()
			.AddSingleton<IConfigurationValidator, XrmSyncConfigurationValidator>()
			.AddSingleton(sp =>
			{
				var config = sp.GetRequiredService<IOptions<XrmSyncConfiguration>>().Value;
				return MSOptions.Create(executionContext with
				{
					DryRun = config.DryRun,
					CiMode = config.CiMode,
					LogLevel = config.LogLevel,
				});
			})
			.AddSingleton(sp => sp.GetRequiredService<IConfigReader>().GetConfiguration())
			.AddSingleton<IConfigurationBuilder, XrmSyncConfigurationBuilder>();

		return services;
	}

	public static IServiceCollection AddOptions(
		this IServiceCollection services,
		Func<XrmSyncConfiguration, XrmSyncConfiguration> configModifier)
	{
		// Register configuration with overloads from command as IOptions<XrmSyncConfiguration>
		services.AddSingleton(sp =>
		{
			var builder = sp.GetRequiredService<IConfigurationBuilder>();
			var baseConfig = builder.Build();

			return MSOptions.Create(configModifier(baseConfig));
		});

		return services;
	}

	/// <summary>
	/// Registers the configuration with the standard execution-level CLI overrides applied
	/// (each falls back to the configured value when the CLI did not supply it).
	/// </summary>
	public static IServiceCollection AddOptions(
		this IServiceCollection services,
		bool? dryRun,
		bool? ciMode,
		LogLevel? logLevel)
		=> services.AddOptions(baseOptions => baseOptions with
		{
			DryRun = dryRun ?? baseOptions.DryRun,
			CiMode = ciMode ?? baseOptions.CiMode,
			LogLevel = logLevel ?? baseOptions.LogLevel,
		});

	public static IServiceCollection AddCommandOptions<TSection>(
		this IServiceCollection services,
		Func<XrmSyncConfiguration, TSection> sectionSelector) where TSection : class
	{
		// Register IOptions<TSection> for services
		services.AddSingleton(sp =>
		{
			var config = sp.GetRequiredService<IOptions<XrmSyncConfiguration>>();
			return MSOptions.Create(sectionSelector(config.Value));
		});

		return services;
	}

	public static IServiceCollection AddLogger(this IServiceCollection services)
	{
		services.AddSingleton(sp =>
		{
			var config = sp.GetRequiredService<IOptions<XrmSyncConfiguration>>();
			return LoggerFactory.Create(
				builder =>
				{
					builder.AddFilter(nameof(Microsoft), LogLevel.Warning)
						.AddFilter(nameof(System), LogLevel.Warning)
						.AddFilter(nameof(XrmSync), config.Value.LogLevel)
						.AddConsole(options => options.FormatterName = "ci-console")
						.AddConsoleFormatter<CIConsoleFormatter, CIConsoleFormatterOptions>(options =>
						{
							options.IncludeScopes = false;
							options.SingleLine = true;
							options.TimestampFormat = "HH:mm:ss ";
							options.CIMode = config.Value.CiMode;
							options.DryRun = config.Value.DryRun;
						});
				});
		});

		services.AddSingleton(typeof(ILogger<>), typeof(SyncLogger<>));

		return services;
	}
}
