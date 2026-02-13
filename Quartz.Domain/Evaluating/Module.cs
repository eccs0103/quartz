using System.Diagnostics.CodeAnalysis;
using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Module(string name, Scope location) : Symbol(name)
{
	public override void Assign(Value value, Range<Position> range)
	{
		throw new NotMutableIssue($"Module '{Name}'", range);
	}

	public bool TryReadClass(string name, [NotNullWhen(true)] out Class? type)
	{
		return location.TryRead(name, out type);
	}

	public bool TryReadTemplate(string name, [NotNullWhen(true)] out Template? template)
	{
		return location.TryRead(name, out template);
	}
}
