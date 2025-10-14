using System.CommandLine;
using XrmSync.Commands;

namespace XrmSync;

internal class CommandLineBuilder
{
    private readonly RootCommand _rootCommand;
    private readonly List<IXrmSyncCommand> _commands;

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
    /// Builds and returns the configured root command
    /// </summary>
    public RootCommand Build()
    {
        return _rootCommand;
    }
}
