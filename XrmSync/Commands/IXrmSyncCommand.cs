using System.CommandLine;

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

	/// <summary>
	/// Returns a <see cref="ProfileOverrideProvider"/> that advertises this command's unique
	/// root-level override options and provides merge logic for applying CLI values into
	/// profile sync items. The <paramref name="assembly"/> and <paramref name="solution"/>
	/// options are shared, owned by the root command — use them in merge callbacks but do
	/// not call Add() on them.
	/// Returns null when the command has no profile overrides to advertise.
	/// </summary>
	ProfileOverrideProvider? GetProfileOverrides(Option<string?> assembly, Option<string?> solution);
}
