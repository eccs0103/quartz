using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Lexing;

public class UnexpectedCharacterIssue(char character, Range<Position> range) : LexingIssue($"Unexpected character '{character}'", range)
{
	public char Character { get; } = character;
}
