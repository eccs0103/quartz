using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Semantic;

public class TypeMismatchIssue(string expected, string actual, Range<Position> range) : SemanticIssue($"Expected type '{expected}', but got '{actual}'", range)
{
	public string Expected { get; } = expected;
	public string Actual { get; } = actual;
}
