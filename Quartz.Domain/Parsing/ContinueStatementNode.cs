using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;
using static Quartz.Domain.Definitions;

namespace Quartz.Domain.Parsing;

public class ContinueStatementNode(Range<Position> range) : Node(range)
{
	public override string ToString()
	{
		return Keywords.Continue;
	}

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
