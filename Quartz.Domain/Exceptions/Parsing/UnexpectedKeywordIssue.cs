using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Parsing;

public class UnexpectedKeywordIssue(string keyword, Range<Position> range) : ParsingIssue($"Unexpected keyword '{keyword}'", range)
{
	public string Keyword { get; } = keyword;
}
