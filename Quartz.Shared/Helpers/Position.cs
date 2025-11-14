namespace Quartz.Shared.Helpers;

public class Position(in uint column, in uint line)
{
	public static Position Zero { get; } = new(0, 0);

	public uint Column { get; } = column;
	public uint Line { get; } = line;

	public override string ToString()
	{
		return $"line {Line + 1} column {Column + 1}";
	}

	public static Range<Position> operator >>(Position from, Position to)
	{
		return new Range<Position>(from, to);
	}

	public static Range<Position> operator ~(Position position)
	{
		return new Range<Position>(position >> position);
	}
}

