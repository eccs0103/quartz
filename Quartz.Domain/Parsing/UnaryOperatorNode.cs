using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class UnaryOperatorNode(IdentifierNode @operator, Node target, Range<Position> range) : OperatorNode(@operator, range)
{
	public Node Target { get; } = target;

	public override string ToString()
	{
		return $"{Operator}{Target}";
	}

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
