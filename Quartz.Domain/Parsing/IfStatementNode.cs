using System.Text;
using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class IfStatementNode(Node condition, Node then, Node? @else, Range<Position> range) : Node(range)
{
	public Node Condition { get; } = condition;
	public Node Then { get; } = then;
	public Node? Else { get; } = @else;

	public override string ToString()
	{
		StringBuilder builder = new();
		builder.Append($"if ({Condition}) ");
		builder.Append(Then);
		if (Else != null)
		{
			builder.Append(" else ");
			builder.Append(Else);
		}
		return builder.ToString();
	}

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
