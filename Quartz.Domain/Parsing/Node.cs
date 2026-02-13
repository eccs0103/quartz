using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public abstract class Node(Range<Position> range)
{
	public Range<Position> RangePosition { get; } = range;
	public abstract T Accept<T>(IEvaluator<T> evaluator, Scope location);
}
