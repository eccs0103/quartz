using System.Diagnostics.CodeAnalysis;
using Quartz.Domain.Exceptions;

namespace Quartz.Domain.Evaluating;

public abstract class Container(string name, Scope location)
{
	public string Name { get; } = name;
	protected Scope Location { get; } = location;

	public bool TryRegister(string name, Value value, bool mutable = false)
	{
		return Location.TryRegister(name, value, mutable);
	}

	public bool TryRead(string name, [NotNullWhen(true)] out Variable? variable)
	{
		return Location.TryRead(name, out variable);
	}
}
