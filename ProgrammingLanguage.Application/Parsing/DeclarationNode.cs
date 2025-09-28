using ProgrammingLanguage.Application.Abstractions;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Parsing;

internal class DeclarationNode(IdentifierNode type, IdentifierNode identifier, Node value, Range<Position> range) : Node(range)
{
	public readonly IdentifierNode Type = type;
	public readonly IdentifierNode Identifier = identifier;
	public readonly Node Value = value;

	public override string ToString()
	{
		return $"({Identifier} {Type}: {Value})";
	}

	public override T Accept<T>(IResolverVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}
