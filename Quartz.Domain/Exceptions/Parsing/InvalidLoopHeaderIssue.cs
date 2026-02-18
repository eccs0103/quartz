using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Parsing;

public class InvalidLoopHeaderIssue(string details, Range<Position> range) : ParsingIssue($"Invalid loop header: {details}", range)
{
	public string Details { get; } = details;
}
