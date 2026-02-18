using System.Diagnostics.CodeAnalysis;
using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Class(string name, Scope location, Class? @base)
{
	public string Name { get; } = name;

	public bool TryRegisterOperator(Operator @operator)
	{
		return location.TryRegister(@operator.Name, new Value<Operator>(TypeConstants.Function, @operator));
	}

	public bool TryReadOperator(string name, [NotNullWhen(true)] out Operator? @operator)
	{
		if (location.TryRead(name, out @operator)) return true;
		if (@base != null) return @base.TryReadOperator(name, out @operator);
		@operator = null;
		return false;
	}

	public bool TryReadOperation(string name, IEnumerable<string> parameters, [NotNullWhen(true)] out Operation? operation)
	{
		if (TryReadOperator(name, out Operator? @operator) && @operator.TryReadOperation(parameters, out operation)) return true;
		if (@base != null) return @base.TryReadOperation(name, parameters, out operation);
		operation = null;
		return false;
	}

	public bool TryRegisterVariable(string name, Value value)
	{
		return location.TryRegister(name, value, true);
	}

	public bool TryRegisterConstant(string name, Value value)
	{
		return location.TryRegister(name, value, false);
	}

	public bool TryReadProperty(string name, [NotNullWhen(true)] out Variable? variable)
	{
		if (location.TryRead(name, out variable)) return true;
		if (@base != null) return @base.TryReadProperty(name, out variable);
		variable = null;
		return false;
	}
}
