using ProgrammingLanguage.Application.Abstractions;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Parsing;

public class BinaryOperatorNode(string @operator, Node left, Node right, Range<Position> range) : OperatorNode(@operator, range)
{
	public readonly Node Left = left;
	public readonly Node Right = right;

	public override string ToString()
	{
		return $"({Left} {Operator} {Right})";
	}

	public override T Accept<T>(IEvaluatorVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}
