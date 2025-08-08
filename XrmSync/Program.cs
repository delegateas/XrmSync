using System.CommandLine;
using XrmSync;

// Build and invoke the root command
var rootCommand = CommandLineBuilder.BuildCommand();
return await rootCommand.InvokeAsync(args);
