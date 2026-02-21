using Quartz.Domain.Evaluating;
using Quartz.Domain.Exceptions.Semantic;
using Quartz.Shared.Extensions;
using Quartz.Shared.Helpers;
using static Quartz.Shared.Constants;

namespace Quartz.Application.Evaluating;

internal delegate Value OperationConfigurator(Value @this, Value[] arguments, Scope scope, Range<Position> range);

internal class ClassBuilder(Class type, Scope location)
{
	public void DeclareVariable(string name, string tag, object value)
	{
		Value variable = new Value<object>(tag, value);
		if (!type.TryRegisterVariable(name, tag, variable)) throw new SymbolAlreadyDeclaredIssue(name, ~Position.Zero);
	}

	public void DeclareConstant(string name, string tag, object value)
	{
		Value constant = new Value<object>(tag, value);
		if (!type.TryRegisterConstant(name, tag, constant)) throw new SymbolAlreadyDeclaredIssue(name, ~Position.Zero);
	}

	public void DeclareOperation(string name, IEnumerable<string> parameters, string result, OperationConfigurator configurator)
	{
		Scope scope = location.GetSubscope(name);
		if (!type.TryReadOperator(name, out Operator? @operator))
		{
			@operator = new Operator(name, location.GetSubscope(name));
			if (!type.TryRegisterOperator(@operator)) throw new SymbolAlreadyDeclaredIssue(name, ~Position.Zero);
		}

		if (type.Name == Types.Workspace)
		{
			Value OperationWrapper(Value[] arguments, Scope scopeCall, Range<Position> range)
			{
				Value workspace = new Value<object>(Types.Workspace, Value.Empty);
				return configurator.Invoke(workspace, arguments, scopeCall, range);
			}
			Operation operation = new(Mangler.Parameters(parameters), parameters, result, OperationWrapper, scope);
			if (!type.TryRegisterOperation(name, operation)) throw new SymbolAlreadyDeclaredIssue(name, ~Position.Zero);
			return;
		}

		parameters = parameters.Prepend(type.Name);
		Value SelfOperationWrapper(Value[] arguments, Scope scopeCall, Range<Position> range)
		{
			return configurator.Invoke(arguments[0], [.. arguments.Skip(1)], scopeCall, range);
		}
		Operation selfOperation = new(Mangler.Parameters(parameters), parameters, result, SelfOperationWrapper, scope);
		if (!type.TryRegisterOperation(name, selfOperation)) throw new SymbolAlreadyDeclaredIssue(name, ~Position.Zero);
	}
}
