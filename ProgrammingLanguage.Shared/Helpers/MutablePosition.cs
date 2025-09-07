namespace ProgrammingLanguage.Shared.Helpers;

public class MutablePosition(uint column, uint line) : Position(column, line)
{
	public MutablePosition(Position position) : this(position.Column, position.Line)
	{
	}

	public new uint Column = column;
	public new uint Line = line;

	public override string ToString()
	{
		return $"line {Line + 1} column {Column + 1}";
	}

	public MutablePosition IncrementColumn()
	{
		Column++;
		return this;
	}

	public MutablePosition IncrementLine()
	{
		Line++;
		Column = 0;
		return this;
	}

	public MutablePosition IncrementBySymbol(char symbol)
	{
		if (symbol == '\n') return IncrementLine();
		return IncrementColumn();
	}

	public Position Seal()
	{
		return new(Column, Line);
	}
}
