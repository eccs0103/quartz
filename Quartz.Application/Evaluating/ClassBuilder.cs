using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Application.Evaluating;

internal class ClassBuilder(Class type, Scope location)
{
	public ClassBuilder DeclareVariable(string name, string tag, object value)
	{
		Instance instance = new(tag, value);
		Datum variable = new(name, tag, instance, true);
		location.Register(name, variable, ~Position.Zero);
		return this;
	}

	public ClassBuilder DeclareConstant(string name, string tag, object value)
	{
		Instance instance = new(tag, value);
		Datum constant = new(name, tag, instance, false);
		location.Register(name, constant, ~Position.Zero);
		return this;
	}

	private Operator GetOperator(string name, Range<Position> range)
	{
		if (type.TryReadOperator(name, out Operator? @operator)) return @operator;
		@operator = new Operator(name, location.GetSubscope(name));
		location.Register(name, @operator, range);
		return @operator;
	}

	public ClassBuilder DeclareOperation(string name, IEnumerable<string> parameters, string result, Func<Instance, Instance[], Scope, Range<Position>, Instance> content)
	{
		Scope scope = location.GetSubscope(name);
		Operator @operator = GetOperator(name, ~Position.Zero);

		if (type.Name == RuntimeBuilder.NameWorkspace)
		{
			Instance Wrapper(Instance[] arguments, Scope scopeCall, Range<Position> range)
			{
				Instance workspace = new(RuntimeBuilder.NameWorkspace, Empty.Instance);
				return content.Invoke(workspace, arguments, scopeCall, range);
			}
			Operation operation = new(Operator.Mangle(parameters), parameters, result, Wrapper, scope);
			@operator.Add(operation, ~Position.Zero);
			return this;
		}

		parameters = parameters.Prepend(type.Name);
		Instance WrapperWithSelf(Instance[] arguments, Scope scopeCall, Range<Position> range)
		{
			return content.Invoke(arguments[0], [.. arguments.Skip(1)], scopeCall, range);
		}
		Operation operationWithSelf = new(Operator.Mangle(parameters), parameters, result, WrapperWithSelf, scope);
		@operator.Add(operationWithSelf, ~Position.Zero);
		return this;
	}
}
