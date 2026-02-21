namespace Quartz.Shared.Helpers;

public class MutablePosition(uint column, uint line) : Position(column, line)
{
	public MutablePosition(Position position) : this(position.Column, position.Line)
	{
	}

	public new uint Column { get; set; } = column;
	public new uint Line { get; set; } = line;

	public override string ToString()
	{
		return $"line {Line + 1} column {Column + 1}";
	}

	private MutablePosition IncrementColumn()
	{
		Column++;
		return this;
	}

	private MutablePosition IncrementLine()
	{
		Line++;
		Column = 0;
		return this;
	}

	public MutablePosition Increment(in char symbol)
	{
		if (symbol == '\n') return IncrementLine();
		return IncrementColumn();
	}

	public MutablePosition Increment(IEnumerable<char> text)
	{
		foreach (char symbol in text) Increment(symbol);
		return this;
	}

	public Position ToImmutable()
	{
		return new(Column, Line);
	}
}
