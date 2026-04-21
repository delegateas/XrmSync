using System.Runtime.CompilerServices;
using XrmSync;
using XrmSync.Commands;
using XrmSync.SyncService;

[assembly: InternalsVisibleTo("Tests")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

Console.WriteLine(new Description().ToolHeader);

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
return await parseResult.InvokeAsync();
