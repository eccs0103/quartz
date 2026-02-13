using Quartz.Domain.Evaluating;
using Quartz.Domain.Exceptions;
using Quartz.Shared.Extensions;
using Quartz.Shared.Helpers;

namespace Quartz.Application.Evaluating;

internal delegate Value OperationContent(Value @this, Value[] arguments, Scope scope, Range<Position> range);

internal class ClassBuilder(Class type, Scope location)
{
	public void DeclareVariable(string name, string tag, object value)
	{
		Value value2 = new Value<object>(tag, value);
		Datum variable = new(name, tag, value2, true);
		if (!location.TryRegister(name, variable)) throw new AlreadyExistsIssue($"Datum '{name}' in {location}", ~Position.Zero);
	}

	public void DeclareConstant(string name, string tag, object value)
	{
		Value value2 = new Value<object>(tag, value);
		Datum constant = new(name, tag, value2, false);
		if (!location.TryRegister(name, constant)) throw new AlreadyExistsIssue($"Datum '{name}' in {location}", ~Position.Zero);
	}

	// TODO: Excess metod
	private Operator GetOperator(string name, Range<Position> range)
	{
		if (type.TryReadOperator(name, out Operator? @operator)) return @operator;
		@operator = new Operator(name, location.GetSubscope(name));
		if (!location.TryRegister(name, @operator)) throw new AlreadyExistsIssue($"Operator'{name}' in {location}", ~Position.Zero);
		return @operator;
	}

	public void DeclareOperation(string name, IEnumerable<string> parameters, string result, OperationContent content)
	{
		Scope scope = location.GetSubscope(name);
		Operator @operator = GetOperator(name, ~Position.Zero);

		if (type.Name == RuntimeBuilder.NameWorkspace)
		{
			Value Wrapper(Value[] arguments, Scope scopeCall, Range<Position> range)
			{
				Value workspace = new Value<object>(RuntimeBuilder.NameWorkspace, Value.Empty);
				return content.Invoke(workspace, arguments, scopeCall, range);
			}
			Operation operation = new(Operator.Mangle(parameters), parameters, result, Wrapper, scope);
			@operator.RegisterOperation(operation, ~Position.Zero);
			return;
		}

		parameters = parameters.Prepend(type.Name);
		Value WrapperWithSelf(Value[] arguments, Scope scopeCall, Range<Position> range)
		{
			return content.Invoke(arguments[0], [.. arguments.Skip(1)], scopeCall, range);
		}
		Operation operationWithSelf = new(Operator.Mangle(parameters), parameters, result, WrapperWithSelf, scope);
		@operator.RegisterOperation(operationWithSelf, ~Position.Zero);
	}
}
