using System.Diagnostics.CodeAnalysis;
using Quartz.Domain.Exceptions;

namespace Quartz.Domain.Evaluating;

public abstract class Container(string name, Scope location)
{
	public string Name { get; } = name;
	protected Scope Location { get; } = location;
}
