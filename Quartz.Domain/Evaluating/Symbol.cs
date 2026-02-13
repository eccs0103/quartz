using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public abstract class Symbol(string name)
{
	public string Name { get; } = name;
	
	public abstract void Assign(Value value, Range<Position> range);
}
