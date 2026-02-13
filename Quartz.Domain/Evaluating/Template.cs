using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Template(string name, IEnumerable<string> generics, Action<Class, IEnumerable<Class>, Scope> builder, Scope location) : Symbol(name)
{
	public override void Assign(Value value, Range<Position> range)
	{
		throw new NotMutableIssue($"Template '{Name}'", range);
	}

	public Class Construct(string name, IEnumerable<Class> arguments, Range<Position> range)
	{
		Scope scope = location.GetSubscope(name);

		using IEnumerator<string> enumeratorGenerics = generics.GetEnumerator();
		using IEnumerator<Class> enumeratorArguments = arguments.GetEnumerator();

		// TODO: Check while run
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

		Class type = new(name, scope, null); // TODO: Add 'Any' base
		builder.Invoke(type, arguments, scope);
		return type;
	}
}
