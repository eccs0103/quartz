using System.Diagnostics.CodeAnalysis;

namespace ProgrammingLanguage.Application.Evaluating;

internal class Registry
{
	private readonly Dictionary<string, Datum> Database = [];

	public object? Read(string name)
	{
		if (!Database.TryGetValue(name, out Datum? datum)) throw new NullReferenceException();
		return datum.Value;
	}

	public void Assign(string name, object? value)
	{
		if (!Database.TryGetValue(name, out Datum? datum)) throw new NullReferenceException();
		if (!datum.Mutable) throw new InvalidOperationException();
		// Type conversation
		datum.Value = value;
		Database[name] = datum;
	}

	public void Declare(string type, string name, object? value, bool mutable)
	{
		Datum constant = new(type, value, mutable);
		if (!Database.TryAdd(name, constant)) throw new InvalidOperationException();
	}

	///


	public bool TryRead(string name, out object? value)
	{
		if (!Database.TryGetValue(name, out Datum? datum))
		{
			value = default!;
			return false;
		}
		value = datum.Value;
		return true;
	}

	public bool TryWrite(string name, object? value)
	{
		if (!Database.TryGetValue(name, out Datum? datum)) return false;
		if (!datum.Mutable) return false;
		datum.Value = value;
		Database[name] = datum;
		return true;
	}

	public bool TryDeclareConstant(string type, string name, object? value, [NotNullWhen(true)] out Datum datum)
	{
		Datum constant = new(type, value, false);
		if (!Database.TryAdd(name, constant))
		{
			datum = default!;
			return false;
		}
		datum = constant;
		return true;
	}

	public bool TryDeclareConstant(string type, string name, out Datum? datum)
	{
		return TryDeclareConstant(type, name, null, out datum);
	}

	public bool TryDeclareVariable(string type, string name, object? value, out Datum? datum)
	{
		Datum constant = new(type, value, true);
		if (!Database.TryAdd(name, constant))
		{
			datum = default!;
			return false;
		}
		datum = constant;
		return true;
	}

	public bool TryDeclareVariable(string type, string name, out Datum? datum)
	{
		return TryDeclareVariable(type, name, null, out datum);
	}

	public bool TryDeclareType(string name, Type equivalent, out Typing? typing)
	{
		Typing constant = new(equivalent);
		if (!Database.TryAdd(name, constant))
		{
			typing = default!;
			return false;
		}
		typing = constant;
		return true;
	}
}
