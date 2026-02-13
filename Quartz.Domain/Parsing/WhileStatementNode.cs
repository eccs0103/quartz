using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class WhileStatementNode(Node condition, Node body, Range<Position> range) : Node(range)
{
	public Node Condition { get; } = condition;
	public Node Body { get; } = body;

	public override string ToString()
	{
		return $"while ({Condition}) {Body}";
	}

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
