using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class FieldNode(Node target, IdentifierNode member, Range<Position> range) : Node(range)
{
	public Node Target { get; } = target;
	public IdentifierNode Member { get; } = member;

	public override string ToString()
	{
		return $"{Target}.{Member}";
	}

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
