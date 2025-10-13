using System.CommandLine;

namespace XrmSync.Commands;

internal class AnalyzeCommandDefinition
{
    public Option<string> AssemblyFile { get; }
    public Option<string> Prefix { get; }
    public Option<bool> PrettyPrint { get; }
    public Option<bool> SaveConfig { get; }
    public Option<string?> SaveConfigTo { get; }

    public AnalyzeCommandDefinition()
    {
        AssemblyFile = new("--assembly", "--assembly-file", "-a", "--af")
        {
            Description = "Path to the plugin assembly (*.dll)",
            Arity = ArgumentArity.ExactlyOne
        };

        Prefix = new("--prefix", "--publisher-prefix", "-p")
        {
            Description = "Publisher prefix for unique names (Default: new)",
            Arity = ArgumentArity.ExactlyOne
        };

        PrettyPrint = new("--pretty-print", "--pp")
        {
            Description = "Pretty print the JSON output",
            Required = false
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
    }

    public IEnumerable<Option> GetOptions()
    {
        yield return AssemblyFile;
        yield return Prefix;
        yield return PrettyPrint;
        yield return SaveConfig;
        yield return SaveConfigTo;
    }
}
