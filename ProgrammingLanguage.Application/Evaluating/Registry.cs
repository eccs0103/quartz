namespace ProgrammingLanguage.Application.Evaluating;

public class Registry
{
	private readonly Dictionary<string, Datum> Database = [];

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

	public bool TryDeclareConstant(string type, string name, object? value, out Datum? datum)
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
