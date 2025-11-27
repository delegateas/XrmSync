using NSubstitute;

namespace Tests;

internal static class Extensions
{
	public static List<T> ArgMatches<T>(this List<T> expected)
	{
		return Arg.Is<List<T>>(actual => !expected.Except(actual).Any() && !actual.Except(expected).Any());
	}
}
