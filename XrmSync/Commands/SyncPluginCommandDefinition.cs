using Microsoft.Extensions.Logging;
using System.CommandLine;

namespace XrmSync.Commands;

internal class SyncPluginCommandDefinition
{
    public Option<string> AssemblyFile { get; }
    public Option<string> SolutionName { get; }
    public Option<bool> DryRun { get; }
    public Option<LogLevel?> LogLevel { get; }
    public Option<bool> SaveConfig { get; }
    public Option<string?> SaveConfigTo { get; }
    public Option<bool> CIMode { get; }

    public SyncPluginCommandDefinition()
    {
        AssemblyFile = new("--assembly", "--assembly-file", "-a", "--af")
        {
            Description = "Path to the plugin assembly (*.dll)",
            Arity = ArgumentArity.ExactlyOne
        };

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

        SaveConfig = new("--save-config", "--sc")
        {
            Description = "Save current CLI options to appsettings.json",
            Required = false
        };

        SaveConfigTo = new("--save-config-to")
        {
            Description = "If --save-config is set, save to this file instead of appsettings.json",
            Required = false
        };

        CIMode = new("--ci", "--ci-mode")
        {
            Description = "Enable CI mode which prefixes all warnings and errors for easier parsing in CI systems",
            Required = false
        };
    }

    public IEnumerable<Option> GetOptions()
    {
        yield return AssemblyFile;
        yield return SolutionName;
        yield return DryRun;
        yield return LogLevel;
        yield return SaveConfig;
        yield return SaveConfigTo;
        yield return CIMode;
    }
}
