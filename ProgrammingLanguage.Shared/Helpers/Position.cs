namespace ProgrammingLanguage.Shared.Helpers;

public class Position(in uint column, in uint line)
{
	public static readonly Position Zero = new(0, 0);

	public readonly uint Column = column;
	public readonly uint Line = line;

	public override string ToString()
	{
		return $"line {Line + 1} column {Column + 1}";
	}
}

