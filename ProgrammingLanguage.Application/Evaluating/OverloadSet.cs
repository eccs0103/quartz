using System.Runtime.CompilerServices;
using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Evaluating;

internal class OverloadSet(string name, Scope location) : Property(name, "OverloadSet", new Dictionary<string, Operation>())
{
	private Dictionary<string, Operation> Operations => Unsafe.As<Dictionary<string, Operation>>(Value);
	private readonly Scope Scope = new(name, location);

	private string Mangle(IEnumerable<string> tags)
	{
		return $"{Name}({string.Join(", ", tags)})";
	}

	public Operation RegisterOperation(IEnumerable<string> parameters, string result, OperationContent function, Range<Position> range)
	{
		string identifier = Mangle(parameters);
		Operation operation = new(identifier, parameters, result, function);
		if (!Operations.TryAdd(identifier, operation)) throw new AlreadyExistsIssue($"Operation '{identifier}' in {Scope}", range);
		return operation;
	}

	public Operation ReadOperation(IEnumerable<string> parameters, Range<Position> range)
	{
		string identifier = Mangle(parameters);
		if (!Operations.TryGetValue(identifier, out Operation? operation)) throw new NotExistIssue($"Operation '{identifier}' in {Scope}", range);
		return operation;
	}
}