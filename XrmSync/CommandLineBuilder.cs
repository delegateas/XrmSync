using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace XrmSync;

internal record Options
{
    public required Option<string> AssemblyFile { get; init; }
    public required Option<string> SolutionName { get; init; }
    public required Option<bool> DryRun { get; init; }
    public required Option<LogLevel?> LogLevel { get; init; }
    public required Option<bool> PrettyPrint { get; init; }
}

internal class CommandLineBuilder
{
    protected RootCommand SyncCommand { get; init; }
    protected Command AnalyzeCommand { get; init; }

    protected Options Options { get; } = new()
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
        DryRun = new("--dry-run", "--dryrun")
        {
            Description = "Perform a dry run without making changes",
            Required = false
        },
        LogLevel = new ("--log-level", "-l")
            {
            Description = "Set the minimum log level (Trace, Debug, Information, Warning, Error, Critical)"
        },
        PrettyPrint = new("--pretty-print", "-p")
        {
            Description = "Pretty print the JSON output",
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
            Options.LogLevel
        };

        AnalyzeCommand = new ("analyze", "Analyze a plugin assembly and output info as JSON")
        {
            Options.AssemblyFile,
            Options.PrettyPrint
        };
        SyncCommand.Subcommands.Add(AnalyzeCommand);
    }

    public CommandLineBuilder SetSyncAction(Func<string?, string?, bool?, LogLevel?, CancellationToken, Task<bool>> syncAction)
    {
        SyncCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var assemblyPath = parseResult.GetValue(Options.AssemblyFile);
            var solutionName = parseResult.GetValue(Options.SolutionName);
            var dryRun = parseResult.GetValue(Options.DryRun);
            var logLevel = parseResult.GetValue(Options.LogLevel);

            return await syncAction(assemblyPath, solutionName, dryRun, logLevel, cancellationToken)
                ? 0
                : 1;
        });

        return this;
    }

    public CommandLineBuilder SetAnalyzeAction(Func<string?, bool, CancellationToken, Task<bool>> analyzeAction)
    {
        AnalyzeCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var assemblyPath = parseResult.GetValue(Options.AssemblyFile);
            var prettyPrint = parseResult.GetValue(Options.PrettyPrint);

            return await analyzeAction(assemblyPath, prettyPrint, cancellationToken)
                ? 0
                : 1;
        });

        return this;
    }

    public RootCommand Build()
    {
        return SyncCommand;
    }
}
