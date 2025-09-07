using ProgrammingLanguage.Application.Abstractions;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Parsing;

public class DeclarationNode(IdentifierNode identifier, Node value, Range<Position> range) : Node(range)
{
	public readonly IdentifierNode Identifier = identifier;
	public readonly Node Value = value;

	public override string ToString()
	{
		return $"(@{Identifier}: {Value})";
	}

	public override T Accept<T>(IEvaluatorVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}
