using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Template(string name, IEnumerable<string> generics, Action<Class, IEnumerable<Class>, Scope> builder, Scope location)
{
	public string Name { get; } = name;

	public Class Assemble(string name, IEnumerable<Class> arguments, Range<Position> range)
	{
		Scope scope = location.GetSubscope(name);
		using IEnumerator<Class> iterator = arguments.GetEnumerator();
		foreach (string generic in generics)
		{
			if (!iterator.MoveNext()) throw new ExpectedIssue($"{generics.Count()} type parameter{(generics.Count() != 1 ? "s" : "")}, but got {arguments.Count()}", range);
			if (!scope.TryRegister(generic, new Value<Class>(TypeConstants.Type, iterator.Current))) throw new AlreadyExistsIssue($"Generic '{generic}' in {scope}", ~Position.Zero);
		}
		if (!location.TryRead(TypeConstants.Any, out Class? typeBase)) throw new NotExistIssue($"Class '{TypeConstants.Any}' in {location}", range);
		Class type = new(name, scope, typeBase);
		builder.Invoke(type, arguments, scope);
		return type;
	}
}
