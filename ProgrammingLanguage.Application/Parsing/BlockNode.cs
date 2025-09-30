using ProgrammingLanguage.Application.Abstractions;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Parsing;

internal class BlockNode(IEnumerable<Node> statements, Range<Position> range) : Node(range)
{
	public readonly IEnumerable<Node> Statements = statements;

	public override string ToString()
	{
		return string.Join('\n', ["{", .. Statements.Select(node => node.ToString()), "}"]);
	}

	public override T Accept<T>(IResolverVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}
