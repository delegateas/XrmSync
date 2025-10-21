using System.CommandLine;
using XrmSync.Commands;

namespace XrmSync;

internal class CommandLineBuilder
{
    private readonly RootCommand _rootCommand;
    private readonly List<IXrmSyncCommand> _commands;
    private XrmSyncRootCommand? _rootCommandHandler;

    public CommandLineBuilder()
    {
        _rootCommand = new ("XrmSync - Synchronize your Dataverse plugins and webresources");
        _commands = [];
    }

    /// <summary>
    /// Adds a command to the root command
    /// </summary>
    public CommandLineBuilder AddCommand(IXrmSyncCommand command)
    {
        _commands.Add(command);
        _rootCommand.Add(command.GetCommand());
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
        _rootCommandHandler = new XrmSyncRootCommand(_commands);
        
        // Copy options from root command handler to the actual root command
        foreach (var option in _rootCommandHandler.Options)
        {
            _rootCommand.Add(option);
        }
        
        // The XrmSyncRootCommand already has its handler set via SetAction in its constructor
        // We just need to invoke it when the root command is called with no subcommand
        _rootCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            // If a subcommand was invoked, don't execute the root handler
            if (parseResult.CommandResult.Command != _rootCommand)
            {
                return 0; // Let the subcommand handle it
            }
            
            // Otherwise, execute the root command handler
            var rootParseResult = _rootCommandHandler.GetCommand().Parse(parseResult.Tokens.Select(t => t.Value).ToArray());
            return await rootParseResult.InvokeAsync(cancellationToken);
        });
        
        return this;
    }

    /// <summary>
    /// Builds and returns the configured root command
    /// </summary>
    public RootCommand Build()
    {
        return _rootCommand;
    }
}
