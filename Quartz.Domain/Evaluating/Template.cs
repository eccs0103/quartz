using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Template(string name, IEnumerable<string> generics, Action<Class, IEnumerable<Class>, Scope> builder, Scope location) : Symbol(name)
{
	public Class Instantiate(string name, IEnumerable<Class> arguments)
	{
		Scope scope = location.GetSubscope(name);
		Class type = new(name, scope);

		using IEnumerator<string> enumeratorGenerics = generics.GetEnumerator();
		using IEnumerator<Class> enumeratorArguments = arguments.GetEnumerator();

		while (enumeratorGenerics.MoveNext())
		{
			if (!enumeratorArguments.MoveNext()) throw new Exception("Invalid generic arguments count");
			type.Define(enumeratorGenerics.Current, enumeratorArguments.Current);
		}

		if (enumeratorArguments.MoveNext()) throw new Exception("Invalid generic arguments count");

		builder.Invoke(type, arguments, scope);
		return type;
	}

	public override void Assign(Instance value, Range<Position> range)
	{
		throw new NotImplementedException();
	}
}
