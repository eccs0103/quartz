using Quartz.Domain.Parsing;

namespace Quartz.Domain.Evaluating;

public static class ValueNodeExtension
{
	internal static T ValueAs<T>(this ValueNode node)
	{
		if (node.Value is T result) return result;
		if (node.Value is null && default(T) is null) return default!;
		string tag = node.Value?.GetType().Name ?? "Null";
		throw new InvalidCastException($"Unable to convert '{node.Value}' from {tag} to {typeof(T).Name}");
	}
}
