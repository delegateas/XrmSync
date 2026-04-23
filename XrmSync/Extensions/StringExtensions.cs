namespace XrmSync.Extensions;

internal static class StringExtensions
{
	public static string GetValueOrDefault(this string? value, string defaultValue) => !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
}
