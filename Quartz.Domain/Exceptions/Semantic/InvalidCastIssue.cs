using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Semantic;

public class InvalidCastIssue(string from, string to, Range<Position> range) : SemanticIssue($"Cannot cast type '{from}' to '{to}'", range)
{
	public string From { get; } = from;
	public string To { get; } = to;
}
