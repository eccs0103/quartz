using Quartz.Domain.Exceptions;
using Quartz.Domain.Parsing;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

internal class Module(string name, Scope location) : Symbol(name)
{
	public override void Assign(ValueNode value, Range<Position> range)
	{
		throw new NotMutableIssue($"Module '{Name}'", range);
	}

	public Class RegisterClass(string name, Scope scope, Range<Position> range)
	{
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
