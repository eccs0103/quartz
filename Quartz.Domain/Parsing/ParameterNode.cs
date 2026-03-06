using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class ParameterNode(IdentifierNode type, IdentifierNode identifier, Range<Position> range) : Node(range)
{
	public IdentifierNode Type { get; } = type;
	public IdentifierNode Identifier { get; } = identifier;

	public override string ToString()
	{
		return $"{Identifier} {Type}";
	}

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
