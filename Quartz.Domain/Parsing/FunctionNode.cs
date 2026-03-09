using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class FunctionNode(IdentifierNode identifier, IEnumerable<ParameterNode> parameters, IdentifierNode result, Node body, Range<Position> range) : Node(range)
{
	public IdentifierNode Identifier { get; } = identifier;
	public IEnumerable<ParameterNode> Parameters { get; } = parameters;
	public IdentifierNode Result { get; } = result;
	public Node Body { get; } = body;

	public override string ToString()
	{
		return $"{Identifier}{Mangler.Parameters(Parameters)} {Result} {Body}";
	}

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
