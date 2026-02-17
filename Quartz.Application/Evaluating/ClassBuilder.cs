using Quartz.Domain.Evaluating;
using Quartz.Domain.Exceptions;
using Quartz.Shared.Extensions;
using Quartz.Shared.Helpers;

namespace Quartz.Application.Evaluating;

internal delegate Value ClassOperationContent(Value @this, Value[] arguments, Scope scope, Range<Position> range);

internal class ClassBuilder(Class type, Scope location)
{
	public void DeclareVariable(string name, string tag, object value)
	{
		Value variable = new Value<object>(tag, value);
		if (!location.TryRegister(name, variable, true)) throw new AlreadyExistsIssue($"Variable '{name}' in {location}", ~Position.Zero);
	}

	public void DeclareConstant(string name, string tag, object value)
	{
		Value constant = new Value<object>(tag, value);
		if (!location.TryRegister(name, constant, false)) throw new AlreadyExistsIssue($"Constant '{name}' in {location}", ~Position.Zero);
	}

	public void DeclareOperation(string name, IEnumerable<string> parameters, string result, ClassOperationContent content)
	{
		Scope scope = location.GetSubscope(name);
		if (!location.TryRead(name, out Operator? @operator))
		{
			@operator = new Operator(name, location.GetSubscope(name));
			if (!location.TryRegister(name, new Value<Operator>(TypeConstants.Function, @operator))) throw new AlreadyExistsIssue($"Operator '{name}' in {location}", ~Position.Zero);
		}

		if (type.Name == RuntimeBuilder.NameWorkspace)
		{
			Value Wrapper(Value[] arguments, Scope scopeCall, Range<Position> range)
			{
				Value workspace = new Value<object>(RuntimeBuilder.NameWorkspace, Value.Empty);
				return content.Invoke(workspace, arguments, scopeCall, range);
			}
			Operation operation = new(name, Mangler.Parameters(parameters), parameters, result, Wrapper, scope);
			@operator.RegisterOperation(operation, ~Position.Zero);
			return;
		}

		parameters = parameters.Prepend(type.Name);
		Value WrapperWithSelf(Value[] arguments, Scope scopeCall, Range<Position> range)
		{
			return content.Invoke(arguments[0], [.. arguments.Skip(1)], scopeCall, range);
		}
		Operation operationWithSelf = new(name, Mangler.Parameters(parameters), parameters, result, WrapperWithSelf, scope);
		@operator.RegisterOperation(operationWithSelf, ~Position.Zero);
	}
}
