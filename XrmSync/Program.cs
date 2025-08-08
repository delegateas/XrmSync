using System.CommandLine;
using XrmSync;

var rootCommand = CommandLineBuilder.BuildCommand();
return await rootCommand.InvokeAsync(args);
