using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Evaluating;

internal class Module(string name, Scope location) : Symbol(name)
{
	public Class RegisterClass(string name, Range<Position> range)
	{
		Scope scope = location.GetSubscope(name);
		Class @class = new(name, scope);
		location.Register(name, @class, range);
		return @class;
	}

	public Class ReadClass(string name, Range<Position> range)
	{
		Symbol symbol = location.Read(name, range);
		if (symbol is not Class @class) throw new NotExistIssue($"Class '{name}' in {location}", range);
		return @class;
	}
}
