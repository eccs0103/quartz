using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Generic(string name, IEnumerable<string> generics, Action<Class, IEnumerable<Class>, Scope> builder, Scope location) : Symbol(name)
{
	public Class Instantiate(string name, IEnumerable<Class> arguments)
	{
		Scope scope = location.GetSubscope(name);

		using IEnumerator<string> enumeratorGenerics = generics.GetEnumerator();
		using IEnumerator<Class> enumeratorArguments = arguments.GetEnumerator();

		while (enumeratorGenerics.MoveNext())
		{
			if (!enumeratorArguments.MoveNext()) throw new Exception("Invalid generic arguments count");
			scope.Register(enumeratorGenerics.Current, enumeratorArguments.Current, ~Position.Zero);
		}

		if (enumeratorArguments.MoveNext()) throw new Exception("Invalid generic arguments count");

		Class type = new(name, scope);
		builder.Invoke(type, arguments, scope);
		return type;
	}

	public override void Assign(Instance value, Range<Position> range)
	{
		throw new NotMutableIssue($"Generic '{Name}'", range);
	}
}
