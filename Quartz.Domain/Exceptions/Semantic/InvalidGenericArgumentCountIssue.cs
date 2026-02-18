using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Semantic;

public class InvalidGenericArgumentCountIssue(int expected, int actual, Range<Position> range) : SemanticIssue($"Expected {expected} generic type parameter(s), but got {actual}", range)
{
	public int Expected { get; } = expected;
	public int Actual { get; } = actual;
}
