using Quartz.Domain.Exceptions.Semantic;
using Quartz.Shared.Helpers;
using static Quartz.Domain.Definitions;

namespace Quartz.Domain.Evaluating;

public delegate void TemplateBuilder(Class type, IEnumerable<Class> parameters, Scope scope);

public class Template(string name, IEnumerable<string> generics, TemplateBuilder builder, Scope location) : Container(name, location)
{
	public Class Assemble(string name, IEnumerable<Class> arguments, Range<Position> range)
	{
		Scope scope = Location.GetSubscope(name);
		using IEnumerator<Class> iterator = arguments.GetEnumerator();
		foreach (string generic in generics)
		{
			if (!iterator.MoveNext()) throw new ArgumentCountIssue(Name, generics.Count(), arguments.Count(), range);
			if (!scope.TryRegister(generic, Types.Type, new Value<Class>(Types.Type, iterator.Current))) throw new SymbolAlreadyDeclaredIssue(generic, ~Position.Zero);
		}
		Class type = new(name, scope, Types.Any);
		builder.Invoke(type, arguments, scope);
		return type;
	}
}
