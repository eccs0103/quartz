using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class IndexNode(Node target, Node index, Range<Position> range) : Node(range)
{
	public Node Target { get; } = target;
	public Node Index { get; } = index;

	public override string ToString()
	{
		return $"{Target}[{Index}]";
	}

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
