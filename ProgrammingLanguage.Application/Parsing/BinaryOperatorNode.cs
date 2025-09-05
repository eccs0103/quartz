using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Parsing;

public partial class BinaryOperatorNode(in string @operator, in Node left, in Node right, in Range<Position> range) : OperatorNode(@operator, range)
{
	public readonly Node Left = left;
	public readonly Node Right = right;
	public override string ToString()
	{
		return $"({Left} {Operator} {Right})";
	}
}
