using System;

namespace ProgrammingLanguage.Application.Evaluating;

public class Datum(in object? value, in Datum.Initializer initializer)
{
	public readonly struct Initializer(in bool mutable = true)
	{
		public readonly bool Mutable = mutable;
	}
	public readonly bool Mutable = initializer.Mutable;
	private object? _Value = value;
	public object? Value
	{
		get => _Value;
		set
		{
			if (!Mutable) return;
			_Value = value;
		}
	}
}
