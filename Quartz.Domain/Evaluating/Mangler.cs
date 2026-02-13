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
}
