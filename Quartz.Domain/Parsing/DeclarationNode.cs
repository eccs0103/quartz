using System.Text;
using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class DeclarationNode(IdentifierNode type, IdentifierNode identifier, Node? value, Range<Position> range) : Node(range)
{
	public IdentifierNode Type { get; } = type;
	public IdentifierNode Identifier { get; } = identifier;
	public Node? Value { get; } = value;

	public override string ToString()
	{
		StringBuilder builder = new($"({Identifier} {Type}");
		if (Value != null) builder.Append($": {Value}");
		return builder.Append(')').ToString();
	}

	public override T Accept<T>(IAstVisitor<T> visitor, Scope location)
	{
		return visitor.Visit(location, this);
	}
}
