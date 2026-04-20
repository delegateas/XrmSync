using System.CommandLine;
using System.CommandLine.Parsing;
using XrmSync.Model;

namespace XrmSync.Commands;

/// <summary>
/// Advertises a command's unique root-level override options and provides merge logic
/// for applying CLI override values into profile sync items.
/// </summary>
internal sealed class ProfileOverrideProvider(
	IReadOnlyList<Option> options,
	Func<SyncItem, ParseResult, SyncItem?> mergeSyncItem)
{
	/// <summary>
	/// Options unique to this command that should be added to the root command.
	/// Shared options (assembly, solution) are owned by the root command, not listed here.
	/// </summary>
	public IReadOnlyList<Option> Options { get; } = options;

	/// <summary>
	/// Merges CLI override values into a sync item.
	/// Returns the merged item if this provider handles the given sync item type,
	/// or null if the item type is not handled by this provider.
	/// </summary>
	public SyncItem? MergeSyncItem(SyncItem item, ParseResult parseResult)
		=> mergeSyncItem(item, parseResult);
}
