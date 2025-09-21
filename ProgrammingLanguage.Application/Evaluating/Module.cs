using ProgrammingLanguage.Application.Abstractions;
using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Evaluating;

internal class Module() : ITypeContainer, IOperationContainer, IDatumContainer
{
	private readonly Dictionary<string, Datum> Database = [];

	public Structure RegisterType(string name, Type equivalent, Range<Position> range)
	{
		Structure type = new(name, equivalent);
		if (!Database.TryAdd(name, type)) throw new AlreadyExistsIssue($"Type '{name}'", range);
		return type;
	}

	public Structure ReadType(string name, Range<Position> range)
	{
		// TODO Change issue type
		if (ReadDatum(name, range) is not Structure type) throw new NotExistIssue($"Type '{name}'", range);
		return type;
	}

	///

	public Operation RegisterOperation(string name, Function function, Range<Position> range)
	{
		Operation operation = new(name, function);
		if (!Database.TryAdd(name, operation)) throw new AlreadyExistsIssue($"Operation '{name}'", range);
		return operation;
	}

	public Operation ReadOperation(string name, Range<Position> range)
	{
		// TODO Change issue type
		if (ReadDatum(name, range) is not Operation operation) throw new NotExistIssue($"Operation '{name}'", range);
		return operation;
	}

	///

	public Datum RegisterConstant(string tag, string name, object? value, Range<Position> range)
	{
		Datum constant = new(tag, name, value);
		if (!Database.TryAdd(name, constant)) throw new AlreadyExistsIssue($"Identifier '{name}'", range);
		return constant;
	}

	public Datum RegisterVariable(string tag, string name, object? value, Range<Position> range)
	{
		Datum variable = new(tag, name, value, true);
		if (!Database.TryAdd(name, variable)) throw new AlreadyExistsIssue($"Identifier '{name}'", range);
		return variable;
	}

	public Datum ReadDatum(string name, Range<Position> range)
	{
		if (!Database.TryGetValue(name, out Datum? datum)) throw new NotExistIssue($"Identifier '{name}'", range);
		return datum;
	}

	public void WriteDatum(string name, string tag, object? value, Range<Position> range)
	{
		Datum datum = ReadDatum(name, range);
		if (!datum.Mutable) throw new NotMutableIssue($"Identifier '{name}'", range);
		if (datum.Tag != tag) throw new TypeMismatchIssue(tag, datum.Tag, range);
		datum.Value = value;
		Database[name] = datum;
	}
}
