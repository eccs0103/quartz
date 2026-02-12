using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Generic(string name, IEnumerable<string> generics, Action<Class, IEnumerable<Class>, Scope> builder, Scope location) : Symbol(name)
{
	public override void Assign(Instance value, Range<Position> range)
	{
		throw new NotMutableIssue($"Generic '{Name}'", range);
	}

	public Class Instantiate(string name, IEnumerable<Class> arguments, Range<Position> range)
	{
		Scope scope = location.GetSubscope(name);

		using IEnumerator<string> enumeratorGenerics = generics.GetEnumerator();
		using IEnumerator<Class> enumeratorArguments = arguments.GetEnumerator();

		int expectedCount = generics.Count();
		int actualCount = arguments.Count();

		if (expectedCount != actualCount)
		{
			throw new ExpectedIssue($"{expectedCount} type parameter{(expectedCount != 1 ? "s" : "")}, but got {actualCount}", range);
		}

		while (enumeratorGenerics.MoveNext())
		{
			enumeratorArguments.MoveNext();
			scope.Register(enumeratorGenerics.Current, enumeratorArguments.Current, ~Position.Zero);
		}

		Class type = new(name, scope);
		builder.Invoke(type, arguments, scope);
		return type;
	}
}
