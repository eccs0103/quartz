using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;
using static Quartz.Shared.Constants;

namespace Quartz.Domain.Parsing;

public class BreakStatementNode(Range<Position> range) : Node(range)
{
	public override string ToString()
	{
		return Keywords.Break;
	}

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
