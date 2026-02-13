using Quartz.Domain.Evaluating;
using Quartz.Shared.Extensions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class InvokationNode(IdentifierNode target, IEnumerable<Node> arguments, Range<Position> range) : Node(range)
{
	public IdentifierNode Target { get; } = target;
	public IEnumerable<Node> Arguments { get; } = arguments;

	public override string ToString()
	{
		return $"{Target}({Arguments.Mangle()})";
	}

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
