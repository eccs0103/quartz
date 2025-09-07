namespace ProgrammingLanguage.Application.Evaluating;

public class Datum
{
	public class Options
	{
		public bool? Mutable { get; set; }

		public void Deconstruct(out bool mutable)
		{
			mutable = Mutable ?? true;
		}
	}

	private static readonly Options ConstantOptions = new() { Mutable = false };
	private static readonly Options VariableOptions = new() { Mutable = true };
	private object? _Value;
	public object? Value
	{
		get => _Value;
		set
		{
			if (!Mutable) return;
			_Value = value;
		}
	}
	public readonly bool Mutable;

	private Datum(object? value, Options options)
	{
		options.Deconstruct(out bool mutable);
		Mutable = mutable;
		_Value = value;
	}

	private Datum(object? value) : this(value, new Options())
	{
	}

	public static Datum ConstantFrom(object? value)
	{
		return new Datum(value, ConstantOptions);
	}

	public static Datum VariableFrom(object? value)
	{
		return new Datum(value, VariableOptions);
	}
}
