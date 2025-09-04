using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Lexing;

public class Token(in Token.Types type, in string value, in Range<Position> range)
{
	public enum Types
	{
		Number,
		String,
		Identifier,
		Keyword,
		Operator,
		Bracket,
		Separator,
	}

	public Types Type { get; } = type;
	public string Value { get; } = value;
	public Range<Position> RangePosition { get; } = range;
	public override string ToString()
	{
		return $"{Type} '{Value}' at {RangePosition.Begin}";
	}
	public bool Match(params string[] values)
	{
		return values.Any(value => value == Value);
	}
	public bool Match(in Types type, params string[] values)
	{
		return type == Type && Match(values);
	}
}
