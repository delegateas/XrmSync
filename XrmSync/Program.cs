using System.CommandLine;
using System.Runtime.CompilerServices;
using XrmSync;
using XrmSync.Commands;
using XrmSync.SyncService;

[assembly: InternalsVisibleTo("Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

var command = new CommandLineBuilder()
	.AddCommands(
		new PluginSyncCommand(),
		new PluginAnalyzeCommand(),
		new WebresourceSyncCommand(),
		new IdentityCommand(),
		new ConfigCommand()
	)
	.WithRootCommandHandler()
	.Build();

var parseResult = command.Parse(args);

// Print the tool header for actual command execution, but not for --help or --version.
// Written to stderr so stdout stays clean for machine-readable output (e.g. analyze JSON).
if (!args.Any(a => a is "--help" or "-h" or "-?" or "--version"))
	Console.Error.WriteLine(new Description().ToolHeader);

// The default termination timeout (~2s) force-kills the process before a sync in flight can finish
// when Ctrl+C is pressed — which matters most in watch mode, where Ctrl+C is the normal way to stop.
return await parseResult.InvokeAsync(new InvocationConfiguration
{
	ProcessTerminationTimeout = TimeSpan.FromSeconds(30)
});
