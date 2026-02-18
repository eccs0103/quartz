using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Parsing;

public class UnexpectedIssue(string found, Range<Position> range) : ParsingIssue($"Unexpected {found}", range)
{
	public string Found { get; } = found;
}
