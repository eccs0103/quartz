using Quartz.Shared.Helpers;

namespace Quartz.Domain.Lexing;

public class Token(Token.Types type, string value, Range<Position> range)
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

	public bool Represents(params string[] values)
	{
		if (values.Length == 0) return true;
		return values.Contains(Value);
	}

	public bool Represents(Types type, params string[] values)
	{
		return type == Type && Represents(values);
	}
}
