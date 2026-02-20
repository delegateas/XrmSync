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

		// Build service provider
		var serviceProvider = GetAnalyzerServices()
			.AddXrmSyncConfiguration(sharedOptions)
			.AddOptions(baseOptions => baseOptions)
			.AddSingleton(sp =>
			{
				var config = sp.GetRequiredService<IOptions<XrmSyncConfiguration>>().Value;

				// Determine assembly path, publisher prefix, and pretty print
				string finalAssemblyPath;
				string finalPublisherPrefix;
				bool finalPrettyPrint;

				// If CLI options provided, use them (standalone mode)
				if (!string.IsNullOrWhiteSpace(assemblyPath) && !string.IsNullOrWhiteSpace(publisherPrefix))
				{
					finalAssemblyPath = assemblyPath;
					finalPublisherPrefix = publisherPrefix;
					finalPrettyPrint = prettyPrint;
				}
				// Otherwise try to get from profile
				else
				{
					var profile = sp.GetRequiredService<IConfigurationBuilder>().GetProfile(sharedOptions.ProfileName) ?? throw new InvalidOperationException(
							$"Profile '{sharedOptions.ProfileName}' not found. " +
							"Either specify --assembly and --publisher-prefix, or use --profile with a valid profile name.");

					var pluginAnalysisItem = profile.Sync.OfType<PluginAnalysisSyncItem>().FirstOrDefault();
					if (pluginAnalysisItem == null)
					{
						throw new InvalidOperationException(
							$"Profile '{profile.Name}' does not contain a PluginAnalysis sync item. " +
							"Either specify --assembly and --publisher-prefix, or use a profile with a PluginAnalysis sync item.");
					}

					finalAssemblyPath = !string.IsNullOrWhiteSpace(assemblyPath)
						? assemblyPath
						: pluginAnalysisItem.AssemblyPath;
					finalPublisherPrefix = !string.IsNullOrWhiteSpace(publisherPrefix)
						? publisherPrefix
						: pluginAnalysisItem.PublisherPrefix;
					finalPrettyPrint = prettyPrint || pluginAnalysisItem.PrettyPrint;
				}

				return Microsoft.Extensions.Options.Options.Create(new PluginAnalysisCommandOptions(finalAssemblyPath, finalPublisherPrefix, finalPrettyPrint));
			})
			.AddLogger()
			.BuildServiceProvider();

		return await RunAction(serviceProvider, ConfigurationScope.PluginAnalysis, CommandAction, cancellationToken)
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
