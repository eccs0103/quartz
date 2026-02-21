using System.Diagnostics.CodeAnalysis;
using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;
using static Quartz.Shared.Constants;

namespace Quartz.Domain.Evaluating;

public class Class(string name, Scope location, Class? @base) : Container(name, location)
{
	public bool TryRegisterOperator(Operator @operator)
	{
		return Location.TryRegister(@operator.Name, Types.Function, new Value<Operator>(Types.Function, @operator), false);
	}

	public bool TryReadOperator(string name, [NotNullWhen(true)] out Operator? @operator)
	{
		if (Location.TryRead(name, out @operator, false)) return true;
		if (@base != null) return @base.TryReadOperator(name, out @operator);
		@operator = null;
		return false;
	}

	public bool TryRegisterOperation(string name, Operation operation)
	{
		if (!TryReadOperator(name, out Operator? @operator))
		{
			@operator = new Operator(name, Location.GetSubscope(name));
			if (!TryRegisterOperator(@operator)) return false;
		}
		return @operator.TryRegisterOperation(operation);
	}

	public bool TryReadOperation(string name, IEnumerable<string> parameters, [NotNullWhen(true)] out Operation? operation)
	{
		if (TryReadOperator(name, out Operator? @operator) && @operator.TryReadOperation(parameters, out operation)) return true;
		if (@base != null) return @base.TryReadOperation(name, parameters, out operation);
		operation = null;
		return false;
	}

	public bool TryRegisterVariable(string name, string tag, Value value)
	{
		return Location.TryRegister(name, tag, value, true);
	}

	public bool TryRegisterConstant(string name, string tag, Value value)
	{
		return Location.TryRegister(name, tag, value, false);
	}

	public bool TryReadProperty(string name, [NotNullWhen(true)] out Variable? variable)
	{
		if (Location.TryRead(name, out variable, false)) return true;
		if (@base != null) return @base.TryReadProperty(name, out variable);
		variable = null;
		return false;
	}
}
