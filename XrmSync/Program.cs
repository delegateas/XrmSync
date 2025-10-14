using System.Runtime.CompilerServices;
using XrmSync;
using XrmSync.Commands;

[assembly: InternalsVisibleTo("Tests")]

var command = new CommandLineBuilder()
    .AddCommands(
        new PluginSyncCommand(),
        new PluginAnalyzeCommand()
    )
    .Build();

var parseResult = command.Parse(args);
return await parseResult.InvokeAsync();
