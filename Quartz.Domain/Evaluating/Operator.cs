using Quartz.Domain.Exceptions;
using Quartz.Domain.Parsing;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

internal class Operator(string name, Scope location) : Symbol(name)
{
	public override void Assign(ValueNode value, Range<Position> range)
	{
		throw new NotMutableIssue($"Operator '{Name}'", range);
	}

	public static string Mangle(string name, IEnumerable<string> tags)
	{
		return $"{name}({string.Join(", ", tags)})";
	}

	public Operation RegisterOperation(IEnumerable<string> parameters, string result, OperationContent function, Scope scope, Range<Position> range)
	{
		Operation operation = new(Mangle(Name, parameters), parameters, result, function, scope);
		location.Register(operation.Name, operation, range);
		return operation;
	}

	public Operation ReadOperation(IEnumerable<string> parameters, Range<Position> range)
	{
		Symbol symbol = location.Read(Mangle(Name, parameters), range);
		if (symbol is not Operation operation) throw new NotExistIssue($"Operation '{symbol.Name}' in {location}", range);
		return operation;
	}
}
