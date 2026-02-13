using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class IdentifierNode(string name, Range<Position> range) : Node(range)
{
	public string Name { get; } = name;

	public override string ToString()
	{
		return Name;
	}

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
