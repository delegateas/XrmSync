using System.CommandLine;
using XrmSync.Commands;

namespace XrmSync;

internal class CommandLineBuilder
{
    private readonly List<IXrmSyncCommand> _commands = [];
    private bool _withRootCommandHandler;

    /// <summary>
    /// Adds a command to the root command
    /// </summary>
    public CommandLineBuilder AddCommand(IXrmSyncCommand command)
    {
        _commands.Add(command);
        return this;
    }

    /// <summary>
    /// Adds multiple commands to the root command
    /// </summary>
    public CommandLineBuilder AddCommands(params IXrmSyncCommand[] commands)
    {
        foreach (var command in commands)
        {
            AddCommand(command);
        }
        return this;
    }

    /// <summary>
    /// Sets up the root command handler to execute all configured sub-commands
    /// </summary>
    public CommandLineBuilder WithRootCommandHandler()
    {
        _withRootCommandHandler = true;

        return this;
    }

    /// <summary>
    /// Builds and returns the configured root command
    /// </summary>
    public RootCommand Build()
    {
        RootCommand rootCommand = [
            .._commands.Select(c => c.GetCommand()), // Register all known sub-commands
        ];
        rootCommand.Description = "XrmSync - Synchronize your Dataverse plugins and webresources";

        if (_withRootCommandHandler)
        {
            XrmSyncRootCommand rootCommandHandler = new (_commands);

            foreach (var option in rootCommandHandler.Options)
            {
                rootCommand.Add(option);
            }

            // The XrmSyncRootCommand already has its handler set via SetAction in its constructor
            // We just need to invoke it when the root command is called with no subcommand
            rootCommand.SetAction(async (parseResult, cancellationToken) =>
            {
                // If a subcommand was invoked, don't execute the root handler
                if (parseResult.CommandResult.Command != rootCommand)
                {
                    return 0; // Let the subcommand handle it
                }

                // Otherwise, execute the root command handler
                var rootParseResult = rootCommandHandler.GetCommand().Parse(parseResult.Tokens.Select(t => t.Value).ToArray());
                return await rootParseResult.InvokeAsync(cancellationToken: cancellationToken);
            });
        }

        return rootCommand;
    }
}
