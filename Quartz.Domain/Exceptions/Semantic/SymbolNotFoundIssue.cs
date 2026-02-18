using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Semantic;

public class SymbolNotFoundIssue(string name, string? expected, Range<Position> range) : SemanticIssue($"{(expected ?? "Symbol")} '{name}' not found", range)
{
	public string Name { get; } = name;
	public string? Expected { get; } = expected;
}
