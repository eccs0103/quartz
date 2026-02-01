using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class BinaryOperatorNode(IdentifierNode @operator, Node left, Node right, Range<Position> range) : OperatorNode(@operator, range)
{
	public Node Left { get; } = left;
	public Node Right { get; } = right;

	private static readonly Dictionary<string, int> Precedence = new()
	{
		{ "*", 3 }, { "/", 3 },
		{ "+", 2 }, { "-", 2 },
		{ ">", 1 }, { "<", 1 }, { ">=", 1 }, { "<=", 1 },
		{ "=", 0 }, { "!=", 0 }
	};

	public override string ToString()
	{
		return Format(this, null);
	}

	private static string Format(Node node, int? target)
	{
		if (node is not BinaryOperatorNode binary) return $"{node}";
		int current = Precedence.GetValueOrDefault(binary.Operator.Name, 0);
		string left = Format(binary.Left, current);
		string right = Format(binary.Right, current);
		string result = $"{left} {binary.Operator} {right}";
		if (current < target) return $"({result})";
		return result;
	}

	public override T Accept<T>(IAstVisitor<T> visitor, Scope location)
	{
		return visitor.Visit(location, this);
	}
}
