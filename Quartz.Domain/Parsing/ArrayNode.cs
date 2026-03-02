using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class ArrayNode(IEnumerable<Node> elements, Range<Position> range) : Node(range)
{
	public IEnumerable<Node> Elements { get; } = elements;

	public override string ToString()
	{
		return Mangler.Enumerations(Elements);
	}

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
