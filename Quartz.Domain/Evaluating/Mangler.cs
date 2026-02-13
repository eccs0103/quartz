using System.Diagnostics.CodeAnalysis;

namespace Quartz.Domain.Evaluating;

public static class Mangler
{
	public static string Parameters(IEnumerable<string> parameters)
	{
		return $"({List(parameters)})";
	}

	public static string Generics(string target, IEnumerable<string> generics)
	{
		return $"{target}<{List(generics)}>";
	}

	public static string List(IEnumerable<object> items)
	{
		return string.Join(", ", items);
	}

	public static bool IsNullable(string tag, [NotNullWhen(true)] out string? inner)
	{
		if (tag.EndsWith('?'))
		{
			inner = tag[..^1];
			return true;
		}
		if (tag.StartsWith("Nullable<") && tag.EndsWith('>'))
		{
			inner = tag[9..^1];
			return true;
		}
		inner = null;
		return false;
	}
}
