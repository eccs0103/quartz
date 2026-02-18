using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public delegate void TemplateBuilder(Class type, IEnumerable<Class> arguments, Scope scope);

public class Template(string name, IEnumerable<string> generics, TemplateBuilder builder, Scope location) : Container(name, location)
{
	public Class Assemble(string name, IEnumerable<Class> arguments, Range<Position> range)
	{
		Scope scope = Location.GetSubscope(name);
		using IEnumerator<Class> iterator = arguments.GetEnumerator();
		foreach (string generic in generics)
		{
			if (!iterator.MoveNext()) throw new ExpectedIssue($"{generics.Count()} type parameter{(generics.Count() != 1 ? "s" : "")}, but got {arguments.Count()}", range);
			if (!scope.TryRegister(generic, new Value<Class>(TypeConstants.Type, iterator.Current))) throw new AlreadyExistsIssue($"Generic '{generic}' in {scope}", ~Position.Zero);
		}
		if (!Location.TryRead(TypeConstants.Any, out Class? typeBase)) throw new NotExistIssue($"Class '{TypeConstants.Any}' in {Location}", range);
		Class type = new(name, scope, typeBase);
		builder.Invoke(type, arguments, scope);
		return type;
	}
}
