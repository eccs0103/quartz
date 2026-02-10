using System.Diagnostics.CodeAnalysis;
using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Class(string name, Scope location, Class? @base) : Symbol(name)
{
	public Scope Location => location;

	public Class(string name, Scope location) : this(name, location, null)
	{
	}

	public override void Assign(Instance value, Range<Position> range)
	{
		throw new NotMutableIssue($"Class '{Name}'", range);
	}

	public Datum ReadProperty(string name, Range<Position> range)
	{
		if (location.TryRead(name, out Symbol? symbol) && symbol is Datum datum) return datum;
		if (@base != null) return @base.ReadProperty(name, range);
		throw new NotExistIssue($"Datum '{name}' in {location}", range);
	}

	public bool TryReadOperator(string name, [NotNullWhen(true)] out Operator? @operator)
	{
		if (location.TryRead(name, out Symbol? symbol) && symbol is Operator operator2)
		{
			@operator = operator2;
			return true;
		}
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
			Operation? operation = @operator.TryReadOperation(parameters);
			if (operation != null) return operation;
		}
		if (@base != null) return @base.ReadOperation(name, parameters, range);
		throw new NoOverloadIssue(name, 0, range);
	}
}
