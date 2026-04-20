using System.CommandLine;

namespace XrmSync.Constants;

/// <summary>
/// Carries option metadata (name, aliases, description) and can create typed Option instances.
/// Eliminates name/alias/description duplication across command constructors.
/// </summary>
internal sealed record CliOptionDescriptor(
	string Primary,
	string[] Aliases,
	string Description,
	ArgumentArity? Arity = null,
	bool AllowMultipleArgumentsPerToken = false)
{
	/// <summary>
	/// Creates an Option&lt;T&gt; from this descriptor. All option metadata comes from one place.
	/// </summary>
	public Option<T> CreateOption<T>(bool required = false)
	{
		var option = new Option<T>(Primary, Aliases)
		{
			Description = Description,
			Required = required,
			AllowMultipleArgumentsPerToken = AllowMultipleArgumentsPerToken
		};
		if (Arity.HasValue)
			option.Arity = Arity.Value;
		return option;
	}
}
