using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class BlockNode(IEnumerable<Node> statements, Range<Position> range) : Node(range)
{
	public IEnumerable<Node> Statements { get; } = statements;

	public override string ToString()
	{
		return string.Join('\n', ["{", .. Statements.Select(node => node.ToString()), "}"]);
	}

	public override T Accept<T>(IAstVisitor<T> visitor, Scope location)
	{
		return visitor.Visit(location, this);
	}
}
