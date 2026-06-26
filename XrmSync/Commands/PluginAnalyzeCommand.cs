using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.CommandLine;
using System.Text.Json;
using XrmSync.Analyzer;
using XrmSync.Analyzer.Extensions;
using XrmSync.Constants;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Model.Plugin;
using XrmSync.Model.Exceptions;
using XrmSync.Options;
using MSOptions = Microsoft.Extensions.Options.Options;

namespace XrmSync.Commands;

internal class PluginAnalyzeCommand : XrmSyncCommandBase
{
	private static readonly Option<bool> PrettyPrint = CliOptions.Analysis.PrettyPrint.CreateOption<bool>();

	public PluginAnalyzeCommand() : base("analyze", "Analyze a plugin assembly and output info as JSON")
	{
		Add(CommandOptions.Assembly);
		Add(CommandOptions.Prefix);
		Add(PrettyPrint);
		AddSharedOptions();

		SetAction(ExecuteAsync);
	}

	public override async Task<int?> ExecuteFromProfile(SyncItem syncItem, ExecutionContext ctx, CancellationToken ct)
	{
		if (syncItem is not PluginAnalysisSyncItem analysis) return null;
		return await RunCore(analysis.AssemblyPath ?? string.Empty, analysis.PublisherPrefix, analysis.PrettyPrint, ctx.ProfileName, ct);
	}

	private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
	{
		var assemblyPath = parseResult.GetValue(CommandOptions.Assembly);
		var publisherPrefix = parseResult.GetValue(CommandOptions.Prefix);
		var prettyPrintValue = parseResult.GetValue(PrettyPrint);
		var profileName = parseResult.GetValue(CommandOptions.Profile);

		// Resolve final options eagerly (CLI + profile merge)
		string finalAssemblyPath;
		string finalPublisherPrefix;
		bool finalPrettyPrint;

		if (profileName == null && !string.IsNullOrWhiteSpace(assemblyPath) && !string.IsNullOrWhiteSpace(publisherPrefix))
		{
			// Standalone mode: all required values supplied via CLI
			finalAssemblyPath = assemblyPath;
			finalPublisherPrefix = publisherPrefix;
			finalPrettyPrint = prettyPrintValue;
		}
		else
		{
			// Profile mode: merge profile values with CLI overrides
			ProfileConfiguration? profile;
			try { profile = LoadProfileAndConfig(profileName).Profile; }
			catch (XrmSyncException ex) { Console.Error.WriteLine(ex.Message); return E_ERROR; }

			if (profile == null)
			{
				Console.Error.WriteLine("No profiles configured. Specify --assembly and --prefix, or add a profile to appsettings.json.");
				return E_ERROR;
			}

			// Sync item is optional — if absent, CLI must supply all analysis-specific values.
			// Assembly path falls back to the profile-level shared value.
			var pluginAnalysisItem = profile.Sync.OfType<PluginAnalysisSyncItem>().FirstOrDefault();

			finalAssemblyPath = assemblyPath.GetValueOrDefault(profile.ResolveAssemblyPath(pluginAnalysisItem?.AssemblyPath) ?? string.Empty);
			finalPublisherPrefix = publisherPrefix.GetValueOrDefault(pluginAnalysisItem?.PublisherPrefix ?? string.Empty);
			finalPrettyPrint = prettyPrintValue || (pluginAnalysisItem?.PrettyPrint ?? false);
		}

		return await RunCore(finalAssemblyPath, finalPublisherPrefix, finalPrettyPrint, profileName, cancellationToken);
	}

	private async Task<int> RunCore(
		string assemblyPath,
		string publisherPrefix,
		bool prettyPrintValue,
		string? profileName,
		CancellationToken ct)
	{
		var errors = XrmSyncConfigurationValidator.ValidateAssemblyPath(assemblyPath)
			.Concat(XrmSyncConfigurationValidator.ValidatePublisherPrefix(publisherPrefix))
			.ToList();
		if (errors.Count > 0)
			return ValidationError("analyze", errors);

		var serviceProvider = GetAnalyzerServices()
			.AddXrmSyncConfiguration(new ExecutionContext(null, null, null, null, profileName))
			.AddOptions(baseOptions => baseOptions)
			.AddSingleton(MSOptions.Create(new PluginAnalysisCommandOptions(assemblyPath, publisherPrefix, prettyPrintValue)))
			.AddLogger()
			.BuildServiceProvider();

		return await RunAction(serviceProvider, ConfigurationScope.None, AnalyzeCommandAction, ct)
			? E_OK
			: E_ERROR;
	}

	private static async Task<bool> AnalyzeCommandAction(IServiceProvider serviceProvider, CancellationToken cancellationToken)
	{
		return await Task.Run(() =>
		{
			try
			{
				var analyzer = serviceProvider.GetRequiredService<IAssemblyAnalyzer>();
				var configuration = serviceProvider.GetRequiredService<IOptions<PluginAnalysisCommandOptions>>();

				var pluginDto = analyzer.AnalyzeAssembly(configuration.Value.AssemblyPath, configuration.Value.PublisherPrefix);
				var jsonOptions = new JsonSerializerOptions(JsonSerializerOptions.Default)
				{
					WriteIndented = configuration.Value.PrettyPrint
				};

				var jsonOutput = JsonSerializer.Serialize(pluginDto, jsonOptions);
				Console.WriteLine(jsonOutput);
				return true;
			}
			catch (XrmSyncException ex)
			{
				Console.Error.WriteLine($"Error analyzing assembly: {ex.Message}");
				return false;
			}
		});
	}

	private static IServiceCollection GetAnalyzerServices(IServiceCollection? services = null)
	{
		services ??= new ServiceCollection();
		services.AddAssemblyAnalyzer();
		return services;
	}
}
