using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Parsing;

public class ExpectedIssue(string expected, Range<Position> range) : ParsingIssue($"Expected {expected}", range)
{
	public string Expected { get; } = expected;
}
