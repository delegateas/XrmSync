using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.CommandLine;
using System.Threading;
using XrmSync.Actions;
using XrmSync.Model.Exceptions;
using XrmSync.Options;

namespace XrmSync;

internal record CommandLineOptions
{
    public required Option<string> AssemblyFile { get; init; }
    public required Option<string> SolutionName { get; init; }
    public required Option<string> Prefix { get; init; }
    public required Option<bool> DryRun { get; init; }
    public required Option<LogLevel?> LogLevel { get; init; }
    public required Option<bool> PrettyPrint { get; init; }
    public required Option<bool> SaveConfig { get; init; }
    public required Option<string?> SaveConfigTo { get; init; }
}

internal record SyncCLIOptions(string? AssemblyPath, string? SolutionName, bool? DryRun, LogLevel? LogLevel);
internal record AnalyzeCLIOptions(string? AssemblyPath, string PublisherPrefix, bool PrettyPrint);

internal class CommandLineBuilder
{
    protected RootCommand SyncCommand { get; init; }
    protected Command AnalyzeCommand { get; init; }

    protected CommandLineOptions Options { get; } = new()
    {
        AssemblyFile = new("--assembly", "-a", "--assembly-file", "--af")
        {
            Description = "Path to the plugin assembly (*.dll)",
            Arity = ArgumentArity.ExactlyOne
        },
        SolutionName = new("--solution-name", "--sn", "-n")
        {
            Description = "Name of the solution",
            Arity = ArgumentArity.ExactlyOne
        },
        Prefix = new("--prefix", "--publisher-prefix", "-p")
        {
            Description = "Publisher prefix for unique names (Default: new)",
            Arity = ArgumentArity.ExactlyOne
        },
        DryRun = new("--dry-run", "--dryrun", "--dr")
        {
            Description = "Perform a dry run without making changes",
            Required = false
        },
        LogLevel = new ("--log-level", "-l")
            {
            Description = "Set the minimum log level (Trace, Debug, Information, Warning, Error, Critical) (Default: Information)"
        },
        PrettyPrint = new("--pretty-print", "--pp")
        {
            Description = "Pretty print the JSON output",
            Required = false
        },
        SaveConfig = new("--save-config", "--sc")
        {
            Description = "Save current CLI options to appsettings.json",
            Required = false
        },
        SaveConfigTo = new ("--save-config-to")
        {
            Description = "If --save-config is set, save to this file instead of appsettings.json",
            Required = false
        }
    };

    public CommandLineBuilder()
    {
        SyncCommand = new ("XrmSync - Synchronize your Dataverse plugins")
        {
            Options.AssemblyFile,
            Options.SolutionName,
            Options.DryRun,
            Options.LogLevel,
            Options.SaveConfig,
            Options.SaveConfigTo
        };

        AnalyzeCommand = new ("analyze", "Analyze a plugin assembly and output info as JSON")
        {
            Options.AssemblyFile,
            Options.Prefix,
            Options.PrettyPrint,
            Options.SaveConfig,
            Options.SaveConfigTo
        };
        SyncCommand.Subcommands.Add(AnalyzeCommand);
    }

    private const int E_OK = 0;
    private const int E_ERROR = 1;

    public CommandLineBuilder SetPluginSyncServiceProviderFactory(Func<SyncCLIOptions, IServiceProvider> factory)
    {
        SyncCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var assemblyPath = parseResult.GetValue(Options.AssemblyFile);
            var solutionName = parseResult.GetValue(Options.SolutionName);
            var dryRun = parseResult.GetValue(Options.DryRun);
            var logLevel = parseResult.GetValue(Options.LogLevel);
            var saveConfig = parseResult.GetValue(Options.SaveConfig);
            var saveConfigTo = saveConfig ? parseResult.GetValue(Options.SaveConfigTo) ?? ConfigReader.CONFIG_FILE_BASE + ".json"  : null;

            var syncOptions = new SyncCLIOptions(assemblyPath, solutionName, dryRun, logLevel);
            var serviceProvider = factory.Invoke(syncOptions);

            return await RunAction(serviceProvider, saveConfigTo, ConfigurationScope.PluginSync, cancellationToken)
                ? E_OK
                : E_ERROR;
        });

        return this;
    }

    public CommandLineBuilder SetPluginAnalyzisServiceProviderFactory(Func<AnalyzeCLIOptions, IServiceProvider> factory)
    {
        AnalyzeCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var assemblyPath = parseResult.GetValue(Options.AssemblyFile);
            var publisherPrefix = parseResult.GetValue(Options.Prefix);
            var prettyPrint = parseResult.GetValue(Options.PrettyPrint);
            var saveConfig = parseResult.GetValue(Options.SaveConfig);
            var saveConfigTo = saveConfig ? parseResult.GetValue(Options.SaveConfigTo) ?? ConfigReader.CONFIG_FILE_BASE + ".json" : null;

            var analyzeOptions = new AnalyzeCLIOptions(assemblyPath, publisherPrefix ?? "new", prettyPrint);
            var serviceProvider = factory.Invoke(analyzeOptions);

            return await RunAction(serviceProvider, saveConfigTo, ConfigurationScope.PluginAnalysis, cancellationToken)
                ? E_OK
                : E_ERROR;
        });

        return this;
    }

    private static async Task<bool> RunAction(IServiceProvider serviceProvider, string? saveConfig, ConfigurationScope configurationScope, CancellationToken cancellationToken)
    {
        // Validate options before taking further action
        try
        {
            var validator = serviceProvider.GetRequiredService<IConfigurationValidator>();
            validator.Validate(configurationScope);
        }
        catch (OptionsValidationException ex)
        {
            Console.Error.WriteLine($"Configuration validation failed:{Environment.NewLine}{ex.Message}");
            return false;
        }

        if (saveConfig is not null)
        {
            var action = serviceProvider.GetRequiredService<ISaveConfigAction>();
            return await action.SaveConfigAsync(saveConfig, cancellationToken);
        }
        else
        {
            var action = serviceProvider.GetRequiredService<IAction>();
            return await action.RunAction(cancellationToken);
        }
    }

    public RootCommand Build()
    {
        return SyncCommand;
    }
}
