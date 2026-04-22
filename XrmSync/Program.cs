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

// Print the tool header for actual command execution, but not for --help or --version
if (!args.Any(a => a is "--help" or "-h" or "-?" or "--version"))
	Console.WriteLine(new Description().ToolHeader);

return await parseResult.InvokeAsync();
