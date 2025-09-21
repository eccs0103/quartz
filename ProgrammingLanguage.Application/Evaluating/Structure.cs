using ProgrammingLanguage.Application.Abstractions;
using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Evaluating;

internal class Structure(string name, Type equivalent) : Datum("Type", name, equivalent), IOperationContainer
{
	private readonly Dictionary<string, Operation> Operations = [];

	public Operation RegisterOperation(string name, Function function, Range<Position> range)
	{
		Operation operation = new(name, function);
		if (!Operations.TryAdd(name, operation)) throw new AlreadyExistsIssue($"Operation '{name}'", range);
		return operation;
	}

	public Operation ReadOperation(string name, Range<Position> range)
	{
		if (!Operations.TryGetValue(name, out Operation? operation)) throw new NotExistIssue($"Operation '{name}'", range);
		return operation;
	}
}