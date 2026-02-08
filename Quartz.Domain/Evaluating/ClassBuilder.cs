using Quartz.Domain.Exceptions;
using Quartz.Domain.Parsing;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

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

	private ClassBuilder DeclareOperation(string name, IEnumerable<string> parameters, string result, Func<ValueNode[], object> content)
	{
		Scope scope = location.GetSubscope(name);
		Operator @operator = GetOperator(name, ~Position.Zero);
		if (type.Name != RuntimeBuilder.NameWorkspace) parameters = parameters.Prepend(type.Name);
		@operator.RegisterOperation(parameters, result, (args, _, range) => new ValueNode(result, content.Invoke(args), range), scope, ~Position.Zero);
		return this;
	}

	public ClassBuilder DeclareOperation(string name, IEnumerable<string> parameters, string result, Action content)
	{
		return DeclareOperation(name, parameters, result, args =>
		{
			content.Invoke();
			return null!;
		});
	}

	public ClassBuilder DeclareOperation<T1>(string name, IEnumerable<string> parameters, string result, Action<T1> content)
	{
		return DeclareOperation(name, parameters, result, args =>
		{
			content.Invoke(args[0].ValueAs<T1>());
			return null!;
		});
	}

	public ClassBuilder DeclareOperation<T1, T2>(string name, IEnumerable<string> parameters, string result, Action<T1, T2> content)
	{
		return DeclareOperation(name, parameters, result, args =>
		{
			content.Invoke(args[0].ValueAs<T1>(), args[1].ValueAs<T2>());
			return null!;
		});
	}

	public ClassBuilder DeclareOperation<T1, T2, T3>(string name, IEnumerable<string> parameters, string result, Action<T1, T2, T3> content)
	{
		return DeclareOperation(name, parameters, result, args =>
		{
			content.Invoke(args[0].ValueAs<T1>(), args[1].ValueAs<T2>(), args[2].ValueAs<T3>());
			return null!;
		});
	}

	public ClassBuilder DeclareOperation<T1, T2, T3, T4>(string name, IEnumerable<string> parameters, string result, Action<T1, T2, T3, T4> content)
	{
		return DeclareOperation(name, parameters, result, args =>
		{
			content.Invoke(args[0].ValueAs<T1>(), args[1].ValueAs<T2>(), args[2].ValueAs<T3>(), args[3].ValueAs<T4>());
			return null!;
		});
	}

	public ClassBuilder DeclareOperation<TResult>(string name, IEnumerable<string> parameters, string result, Func<TResult> content)
	{
		return DeclareOperation(name, parameters, result, args => content.Invoke()!);
	}

	public ClassBuilder DeclareOperation<T1, TResult>(string name, IEnumerable<string> parameters, string result, Func<T1, TResult> content)
	{
		return DeclareOperation(name, parameters, result, args => content.Invoke(args[0].ValueAs<T1>())!);
	}

	public ClassBuilder DeclareOperation<T1, T2, TResult>(string name, IEnumerable<string> parameters, string result, Func<T1, T2, TResult> content)
	{
		return DeclareOperation(name, parameters, result, args => content.Invoke(args[0].ValueAs<T1>(), args[1].ValueAs<T2>())!);
	}

	public ClassBuilder DeclareOperation<T1, T2, T3, TResult>(string name, IEnumerable<string> parameters, string result, Func<T1, T2, T3, TResult> content)
	{
		return DeclareOperation(name, parameters, result, args => content.Invoke(args[0].ValueAs<T1>(), args[1].ValueAs<T2>(), args[2].ValueAs<T3>())!);
	}

	public ClassBuilder DeclareOperation<T1, T2, T3, T4, TResult>(string name, IEnumerable<string> parameters, string result, Func<T1, T2, T3, T4, TResult> content)
	{
		return DeclareOperation(name, parameters, result, args => content.Invoke(args[0].ValueAs<T1>(), args[1].ValueAs<T2>(), args[2].ValueAs<T3>(), args[3].ValueAs<T4>())!);
	}
}
