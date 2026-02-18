using System.Diagnostics.CodeAnalysis;
using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Class(string name, Scope location, Class? @base) : Container(name, location)
{
	public bool TryRegisterOperator(Operator @operator)
	{
		return Location.TryRegister(@operator.Name, new Value<Operator>(TypeConstants.Function, @operator));
	}

	public bool TryReadOperator(string name, [NotNullWhen(true)] out Operator? @operator)
	{
		if (Location.TryRead(name, out @operator, deep: false)) return true;
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
		return Location.TryRegister(name, value, true);
	}

	public bool TryRegisterConstant(string name, Value value)
	{
		return Location.TryRegister(name, value, false);
	}

	public bool TryReadProperty(string name, [NotNullWhen(true)] out Variable? variable)
	{
		if (Location.TryRead(name, out variable, deep: false)) return true;
		if (@base != null) return @base.TryReadProperty(name, out variable);
		variable = null;
		return false;
	}
}
