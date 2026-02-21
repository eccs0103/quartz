using Quartz.Domain.Evaluating;
using Quartz.Shared.Extensions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class InvocationNode(Node target, IEnumerable<Node> arguments, Range<Position> range) : Node(range)
{
	public Node Target { get; } = target;
	public IEnumerable<Node> Arguments { get; } = arguments;

	public override string ToString()
	{
		return $"{Target}({Mangler.List(Arguments)})";
	}

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
