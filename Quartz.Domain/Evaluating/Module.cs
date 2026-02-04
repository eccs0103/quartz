using System.Diagnostics.CodeAnalysis;
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

	public Class RegisterClass(string name, Class? @base, Scope scope, Range<Position> range)
	{
		Class @class = new(name, scope, @base);
		location.Register(name, @class, range);
		return @class;
	}

	public Class ReadClass(string name, Range<Position> range)
	{
		if (TryReadClass(name, out Class? @class)) return @class;
		throw new NotExistIssue($"Class '{name}' in {location}", range);
	}

	public bool TryReadClass(string name, [NotNullWhen(true)] out Class? @class)
	{
		if (location.TryRead(name, out Symbol? symbol) && symbol is Class class2)
		{
			@class = class2;
			return true;
		}
		@class = null;
		return false;
	}
}
