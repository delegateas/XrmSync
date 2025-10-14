using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace XrmSync.Commands;

/// <summary>
/// Interface for self-contained XrmSync commands that can be added to the root command
/// </summary>
internal interface IXrmSyncCommand
{
    /// <summary>
    /// Gets the command instance with all options and handlers configured
    /// </summary>
    Command GetCommand();
}
