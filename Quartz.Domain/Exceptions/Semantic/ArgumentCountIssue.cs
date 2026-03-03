using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Semantic;

public class ArgumentCountIssue(string context, int expected, int actual, Range<Position> range)
	: SemanticIssue($"'{context}' expects {expected} argument(s), but got {actual}", range)
{
	public string Context { get; } = context;
	public int Expected { get; } = expected;
	public int Actual { get; } = actual;
}
