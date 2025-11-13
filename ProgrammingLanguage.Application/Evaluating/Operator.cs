using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Evaluating;

internal class Operator(string name, Scope location) : Symbol(name)
{
	private static string Mangle(string name, IEnumerable<string> tags)
	{
		return $"{name}({string.Join(", ", tags)})";
	}

	public Operation RegisterOperation(IEnumerable<string> parameters, string result, OperationContent function, Range<Position> range)
	{
		string name = Mangle(Name, parameters);
		Scope scope = location.GetSubscope(name);
		Operation operation = new(name, parameters, result, function, scope);
		location.Register(name, operation, range);
		return operation;
	}

	public Operation ReadOperation(IEnumerable<string> parameters, Range<Position> range)
	{
		string name = Mangle(Name, parameters);
		Symbol symbol = location.Read(name, range);
		if (symbol is not Operation operation) throw new NotExistIssue($"Operation '{name}' in {location}", range);
		return operation;
	}
}
