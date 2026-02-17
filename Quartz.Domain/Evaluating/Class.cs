using System.Diagnostics.CodeAnalysis;
using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Class(string name, Scope location, Class? @base)
{
	public string Name { get; } = name;

	public Variable ReadProperty(string name, Range<Position> range)
	{
		if (location.TryRead(name, out Variable? variable)) return variable;
		if (@base != null) return @base.ReadProperty(name, range);
		throw new NotExistIssue($"Variable '{name}' in {location}", range);
	}

	public bool TryReadOperator(string name, [NotNullWhen(true)] out Operator? @operator)
	{
		if (location.TryRead(name, out @operator)) return true;
		if (@base != null) return @base.TryReadOperator(name, out @operator);
		@operator = null;
		return false;
	}

	public Operator ReadOperator(string name, Range<Position> range)
	{
		if (TryReadOperator(name, out Operator? @operator)) return @operator;
		throw new NotExistIssue($"Operator '{name}' in {location}", range);
	}

	public Operation ReadOperation(string name, IEnumerable<string> parameters, Range<Position> range)
	{
		if (TryReadOperator(name, out Operator? @operator))
		{
			if (@operator.TryReadOperation(parameters, out Operation? operation)) return operation;
		}
		if (@base != null) return @base.ReadOperation(name, parameters, range);
		throw new NoOverloadIssue(name, 0, range);
	}
}
