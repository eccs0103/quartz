using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class ForStatementNode(IdentifierNode identifier, IdentifierNode type, Node collection, Node body, Range<Position> range) : Node(range)
{
	public IdentifierNode Identifier { get; } = identifier;
	public IdentifierNode Type { get; } = type;
	public Node Collection { get; } = collection;
	public Node Body { get; } = body;

	public override string ToString()
	{
		return $"for ({Identifier} {Type} in {Collection}) {Body}";
	}

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
