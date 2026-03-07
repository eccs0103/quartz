using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class ReturnNode(Node? value, Range<Position> range) : Node(range)
{
	public Node? Value { get; } = value;

	public override string ToString()
	{
		if (Value == null) return Definitions.Keywords.Return;
		return $"{Definitions.Keywords.Return} {Value}";
	}

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
