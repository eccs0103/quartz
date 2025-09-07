using ProgrammingLanguage.Application.Abstractions;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Parsing;

public class IdentifierNode(string name, Range<Position> range) : Node(range)
{
	public readonly string Name = name;

	public override string ToString()
	{
		return $"{Name}";
	}

	public override T Accept<T>(IEvaluatorVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}
