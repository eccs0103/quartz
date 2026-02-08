using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public abstract class Symbol(string name)
{
	public string Name { get; } = name;
	public abstract void Assign(Instance value, Range<Position> range);
}
