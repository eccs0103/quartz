using ProgrammingLanguage.Application.Abstractions;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Parsing;

public class InvokationNode(IdentifierNode target, IEnumerable<Node> arguments, Range<Position> range) : Node(range)
{
	public readonly IdentifierNode Target = target;
	public readonly IEnumerable<Node> Arguments = arguments;

	public override string ToString()
	{
		return $"{Target}({string.Join(", ", Arguments)})";
	}

	public override T Accept<T>(IEvaluatorVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}
