using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Semantic;

public class InvalidSymbolUsageIssue(string name, string expected, Range<Position> range) : SemanticIssue($"Symbol '{name}' is not a {expected}", range)
{
	public string Name { get; } = name;
	public string Expected { get; } = expected;
}
