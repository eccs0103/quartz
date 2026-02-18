using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Semantic;

public class ArgumentCountMismatchIssue(string target, int expected, int actual, Range<Position> range) : SemanticIssue($"Function '{target}' expects {expected} argument(s), but got {actual}", range)
{
	public string Target { get; } = target;
	public int Expected { get; } = expected;
	public int Actual { get; } = actual;
}
