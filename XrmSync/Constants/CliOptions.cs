namespace XrmSync.Constants;

/// <summary>
/// Centralized constants for all CLI option names and aliases
/// </summary>
internal static class CliOptions
{
    /// <summary>
    /// Assembly/Plugin options
    /// </summary>
    internal static class Assembly
    {
        public const string Primary = "--assembly";
        public static readonly string[] Aliases = ["--assembly-file", "-a", "--af"];
        public const string Description = "Path to the plugin assembly (*.dll)";
    }

    /// <summary>
    /// Webresource folder options
    /// </summary>
    internal static class Webresource
    {
        public const string Primary = "--folder";
        public static readonly string[] Aliases = ["--webresources", "-w", "--wr", "--path"];
        public const string Description = "Path to the root folder containing the webresources to sync";
    }

    /// <summary>
    /// Solution name options
    /// </summary>
    internal static class Solution
    {
        public const string Primary = "--solution";
        public static readonly string[] Aliases = ["--solution-name", "--sn", "-n"];
        public const string Description = "Name of the solution";
    }

    /// <summary>
    /// Execution mode options
    /// </summary>
    internal static class Execution
    {
        internal static class DryRun
        {
            public const string Primary = "--dry-run";
            public static readonly string[] Aliases = ["--dryrun", "--dr"];
            public const string Description = "Perform a dry run without making changes to Dataverse";
        }
    }

    /// <summary>
    /// CI and logging options
    /// </summary>
    internal static class Logging
    {
        internal static class CiMode
        {
            public const string Primary = "--ci-mode";
            public static readonly string[] Aliases = ["--ci"];
            public const string Description = "Enable CI mode which prefixes all warnings and errors for easier parsing in CI systems";
        }

        internal static class LogLevel
        {
            public const string Primary = "--log-level";
            public static readonly string[] Aliases = ["-l", "--ll"];
            public const string Description = "Set the minimum log level (Trace, Debug, Information, Warning, Error, Critical) (Default: Information)";
        }
    }

    /// <summary>
    /// Configuration file options
    /// </summary>
    internal static class Config
    {
        public static class SaveConfig
        {
            public const string Primary = "--save-config";
            public static readonly string[] Aliases = ["--sc"];
            public const string Description = "Save current CLI options to appsettings.json";
        }


        public static class SaveConfigTo
        {
            public const string Primary = "--save-config-to";
            public const string Description = $"If {SaveConfig.Primary} is set, save to this file instead of appsettings.json";
        }

        public static class LoadConfig
        {
            public const string Primary = "--config";
            public static readonly string[] Aliases = ["--config-name", "-c"];
            public const string Description = "Name of the configuration to load from appsettings.json (Default: 'default' or single config if only one exists)";
        }
    }

    /// <summary>
    /// Analysis-specific options
    /// </summary>
    internal static class Analysis
    {
        public static class Prefix
        {
            public const string Primary = "--prefix";
            public static readonly string[] Aliases = ["--publisher-prefix", "-p"];
            public const string Description = "Publisher prefix for unique names (Default: new)";
        }

        public static class PrettyPrint
        {
            public const string Primary = "--pretty-print";
            public static readonly string[] Aliases = ["--pp"];
            public const string Description = "Pretty print the JSON output";
        }
    }
}
