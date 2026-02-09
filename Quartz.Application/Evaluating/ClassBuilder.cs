using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;
using Quartz.Domain.Evaluating;

namespace Quartz.Application.Evaluating;

internal class ClassBuilder(Class type, Scope location)
{
	public ClassBuilder DeclareVariable(string name, string tag, object value)
	{
		type.RegisterVariable(name, tag, value, ~Position.Zero);
		return this;
	}

	public ClassBuilder DeclareConstant(string name, string tag, object value)
	{
		type.RegisterConstant(name, tag, value, ~Position.Zero);
		return this;
	}

	private Operator GetOperator(string name, Range<Position> range)
	{
		try
		{
			return type.RegisterOperator(name, range);
		}
		catch (AlreadyExistsIssue)
		{
			return type.ReadOperator(name, range);
		}
	}

	public ClassBuilder DeclareOperation(string name, IEnumerable<string> parameters, string result, Func<Instance, Instance[], Scope, Range<Position>, Instance> content)
	{
		Scope scope = location.GetSubscope(name);
		Operator @operator = GetOperator(name, ~Position.Zero);

		if (type.Name == RuntimeBuilder.NameWorkspace)
		{
			@operator.RegisterOperation(parameters, result, (arguments, scopeCall, range) =>
			{
				Instance workspace = new(RuntimeBuilder.NameWorkspace, Null.Instance, scopeCall);
				return content.Invoke(workspace, arguments, scopeCall, range);
			}, scope, ~Position.Zero);
			return this;
		}

		parameters = parameters.Prepend(type.Name);
		@operator.RegisterOperation(parameters, result, (arguments, scopeCall, range) =>
		{
			return content.Invoke(arguments[0], [.. arguments.Skip(1)], scopeCall, range);
		}, scope, ~Position.Zero);
		return this;
	}
}
