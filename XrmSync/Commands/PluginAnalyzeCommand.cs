using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.CommandLine;
using System.Text.Json;
using XrmSync.Analyzer;
using XrmSync.Analyzer.Extensions;
using XrmSync.Constants;
using XrmSync.Extensions;
using XrmSync.Model;
using XrmSync.Model.Exceptions;
using XrmSync.Options;
using MSOptions = Microsoft.Extensions.Options.Options;

namespace XrmSync.Commands;

internal class PluginAnalyzeCommand : XrmSyncCommandBase
{

	private readonly Option<string> assemblyFile;
	private readonly Option<string> prefix;
	private readonly Option<bool> prettyPrint;

	public PluginAnalyzeCommand() : base("analyze", "Analyze a plugin assembly and output info as JSON")
	{
		assemblyFile = new(CliOptions.Assembly.Primary, CliOptions.Assembly.Aliases)
		{
			Description = CliOptions.Assembly.Description,
			Arity = ArgumentArity.ZeroOrOne
		};

		prefix = new(CliOptions.Analysis.Prefix.Primary, CliOptions.Analysis.Prefix.Aliases)
		{
			Description = CliOptions.Analysis.Prefix.Description,
			Arity = ArgumentArity.ZeroOrOne
		};

		prettyPrint = new(CliOptions.Analysis.PrettyPrint.Primary, CliOptions.Analysis.PrettyPrint.Aliases)
		{
			Description = CliOptions.Analysis.PrettyPrint.Description,
			Required = false
		};

		Add(assemblyFile);
		Add(prefix);
		Add(prettyPrint);
		AddSharedOptions();

		SetAction(ExecuteAsync);
	}

	private async Task<int> ExecuteAsync(ParseResult parseResult, CancellationToken cancellationToken)
	{
		var assemblyPath = parseResult.GetValue(assemblyFile);
		var publisherPrefix = parseResult.GetValue(prefix);
		var prettyPrint = parseResult.GetValue(this.prettyPrint);
		var sharedOptions = GetSharedOptionValues(parseResult);

		// Resolve final options eagerly (CLI + profile merge)
		string finalAssemblyPath;
		string finalPublisherPrefix;
		bool finalPrettyPrint;

		if (sharedOptions.ProfileName == null && !string.IsNullOrWhiteSpace(assemblyPath) && !string.IsNullOrWhiteSpace(publisherPrefix))
		{
			// Standalone mode: all required values supplied via CLI
			finalAssemblyPath = assemblyPath;
			finalPublisherPrefix = publisherPrefix;
			finalPrettyPrint = prettyPrint;
		}
		else
		{
			// Profile mode: merge profile values with CLI overrides
			ProfileConfiguration? profile;
			try { profile = LoadProfile(sharedOptions.ProfileName); }
			catch (XrmSyncException ex) { Console.Error.WriteLine(ex.Message); return E_ERROR; }

			var pluginAnalysisItem = profile?.Sync.OfType<PluginAnalysisSyncItem>().FirstOrDefault();
			if (profile == null || pluginAnalysisItem == null)
			{
				Console.Error.WriteLine(
					profile == null
						? "No profiles configured. Specify --assembly and --prefix, or add a profile to appsettings.json."
						: $"Profile '{profile.Name}' does not contain a PluginAnalysis sync item. Specify --assembly and --prefix, or add a PluginAnalysis sync item to the profile.");
				return E_ERROR;
			}

			finalAssemblyPath = !string.IsNullOrWhiteSpace(assemblyPath) ? assemblyPath : pluginAnalysisItem.AssemblyPath;
			finalPublisherPrefix = !string.IsNullOrWhiteSpace(publisherPrefix) ? publisherPrefix : pluginAnalysisItem.PublisherPrefix;
			finalPrettyPrint = prettyPrint || pluginAnalysisItem.PrettyPrint;
		}

		// Validate resolved values
		var errors = XrmSyncConfigurationValidator.ValidateAssemblyPath(finalAssemblyPath)
			.Concat(XrmSyncConfigurationValidator.ValidatePublisherPrefix(finalPublisherPrefix))
			.ToList();
		if (errors.Count > 0)
			return ValidationError("analyze", errors);

		// Build service provider with validated options
		var serviceProvider = GetAnalyzerServices()
			.AddXrmSyncConfiguration(sharedOptions)
			.AddOptions(baseOptions => baseOptions)
			.AddSingleton(MSOptions.Create(new PluginAnalysisCommandOptions(finalAssemblyPath, finalPublisherPrefix, finalPrettyPrint)))
			.AddLogger()
			.BuildServiceProvider();

		return await RunAction(serviceProvider, ConfigurationScope.None, CommandAction, cancellationToken)
			? E_OK
			: E_ERROR;
	}

	private static async Task<bool> CommandAction(IServiceProvider serviceProvider, CancellationToken cancellationToken)
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
