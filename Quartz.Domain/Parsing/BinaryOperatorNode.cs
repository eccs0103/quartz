using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class BinaryOperatorNode(IdentifierNode @operator, Node left, Node right, Range<Position> range) : OperatorNode(@operator, range)
{
	public Node Left { get; } = left;
	public Node Right { get; } = right;

	public override string ToString()
	{
		return $"({Left} {Operator} {Right})";
	}

	public override T Accept<T>(IAstVisitor<T> visitor, Scope location)
	{
		return visitor.Visit(location, this);
	}
}
