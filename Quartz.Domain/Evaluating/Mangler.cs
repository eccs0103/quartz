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

	public static bool IsGeneric(string tag, [NotNullWhen(true)] out string? template, [NotNullWhen(true)] out IEnumerable<string>? arguments)
	{
		if (!tag.EndsWith('>'))
		{
			template = null;
			arguments = null;
			return false;
		}

		int separator = tag.IndexOf('<');
		if (separator <= 0)
		{
			template = null;
			arguments = null;
			return false;
		}

		template = tag[..separator];
		string content = tag[(separator + 1)..^1];

		List<string> found = [];
		int brackets = 0;
		int start = 0;
		for (int cursor = 0; cursor < content.Length; cursor++)
		{
			if (content[cursor] == '<')
			{
				brackets++;
				continue;
			}
			if (content[cursor] == '>')
			{
				brackets--;
				continue;
			}
			if (content[cursor] == ',' && brackets == 0)
			{
				found.Add(content[start..cursor].Trim());
				start = cursor + 1;
			}
		}
		found.Add(content[start..].Trim());
		arguments = [.. found];

		return true;
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
