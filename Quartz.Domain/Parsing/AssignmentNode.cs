using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class AssignmentNode(IdentifierNode identifier, Node value, Range<Position> range) : Node(range)
{
	public IdentifierNode Identifier { get; } = identifier;
	public Node Value { get; } = value;

	public override string ToString()
	{
		return $"({Identifier}: {Value})";
	}

	public override T Accept<T>(IAstVisitor<T> visitor, Scope location)
	{
		return visitor.Visit(location, this);
	}
}

