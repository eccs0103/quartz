namespace ProgrammingLanguage.Application.Evaluating;

public class Datum(string type, object? value, bool mutable)
{
	public readonly string Type = type;
	private object? _Value = value;
	public readonly bool Mutable = mutable;

	public object? Value
	{
		get => _Value;
		set
		{
			if (!Mutable) return;
			_Value = value;
		}
	}

	public Datum(string type, object? value) : this(type, value, false)
	{
	}
}
