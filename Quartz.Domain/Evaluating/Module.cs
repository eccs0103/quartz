using System.Diagnostics.CodeAnalysis;
using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Module(string name, Scope location)
{
	public string Name { get; } = name;

	public bool TryReadClass(string name, [NotNullWhen(true)] out Class? type)
	{
		return location.TryRead(name, out type);
	}

	public Class ReadClass(string name, Range<Position> range)
	{
		if (TryReadClass(name, out Class? type)) return type;
		throw new NotExistIssue($"Class '{name}' in {location}", range);
	}

	public bool TryReadTemplate(string name, [NotNullWhen(true)] out Template? template)
	{
		return location.TryRead(name, out template);
	}

	public Template ReadTemplate(string name, Range<Position> range)
	{
		if (TryReadTemplate(name, out Template? template)) return template;
		throw new NotExistIssue($"Template '{name}' in {location}", range);
	}
}
