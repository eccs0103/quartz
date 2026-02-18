using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Parsing;

public class UnmatchedBracketIssue(string bracket, Range<Position> range) : ParsingIssue($"Unmatched bracket '{bracket}'", range)
{
	public string Bracket { get; } = bracket;
}
