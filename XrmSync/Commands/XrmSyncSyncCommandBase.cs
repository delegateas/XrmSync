using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace XrmSync.Commands
{
    internal abstract class XrmSyncSyncCommandBase(string name, string description) : XrmSyncCommandBase(name, description)
    {
        protected Option<string> SolutionName { get; private set; } = null!;
        protected Option<bool> DryRun {get; private set; } = null!;
        protected Option<LogLevel?> LogLevel {get; private set; } = null!;
        protected Option<bool> CiMode {get; private set; } = null!;

        protected virtual void AddSyncSharedOptions()
        {
            SolutionName = new("--solution", "--solution-name", "--sn", "-n")
            {
                Description = "Name of the solution",
                Arity = ArgumentArity.ExactlyOne
            };

            DryRun = new("--dry-run", "--dryrun", "--dr")
            {
                Description = "Perform a dry run without making changes",
                Required = false
            };

            LogLevel = new("--log-level", "-l")
            {
                Description = "Set the minimum log level (Trace, Debug, Information, Warning, Error, Critical) (Default: Information)"
            };

            CiMode = new("--ci", "--ci-mode")
            {
                Description = "Enable CI mode which prefixes all warnings and errors for easier parsing in CI systems",
                Required = false
            };

            Add(SolutionName);
            Add(DryRun);
            Add(LogLevel);
            Add(CiMode);
        }

        protected (string? SolutionName, bool DryRun, LogLevel? LogLevel, bool CIMode) GetSyncSharedOptionValues(ParseResult parseResult)
        {
            var solutionName = parseResult.GetValue(SolutionName);
            var dryRun = parseResult.GetValue(DryRun);
            var logLevel = parseResult.GetValue(LogLevel);
            var ciMode = parseResult.GetValue(CiMode);

            return (solutionName, dryRun, logLevel, ciMode);
        }
    }
}
