namespace ProgrammingLanguage.Application.Evaluating;

internal class Datum(string tag, string name, object? value, bool mutable)
{
	public readonly string Tag = tag;
	public readonly string Name = name;
	private object? _Value = value;
	public readonly bool Mutable = mutable;

	public object? Value
	{
		get => _Value;
		set
		{
			if (!Mutable) throw new InvalidOperationException("Unable to modify immutable value");
			_Value = value;
		}
	}

	public Datum(string tag, string name, object? value) : this(tag, name, value, false)
	{
	}
}
