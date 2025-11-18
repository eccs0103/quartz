using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class WhileStatementNode(Node count, Node body, Range<Position> range) : Node(range)
{
	public Node Count { get; } = count;
	public Node Body { get; } = body;

	public override string ToString()
	{
		throw new NotImplementedException();
	}

	public override T Accept<T>(IAstVisitor<T> visitor, Scope location)
	{
		return visitor.Visit(location, this);
	}
}
