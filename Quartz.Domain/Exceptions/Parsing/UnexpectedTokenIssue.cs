using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Parsing;

public class UnexpectedTokenIssue(string token, Range<Position> range) : ParsingIssue($"Unexpected token '{token}'", range)
{
	public string Token { get; } = token;
}
