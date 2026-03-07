using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;
using static Quartz.Domain.Definitions;

namespace Quartz.Domain.Parsing;

public class BinaryOperatorNode(IdentifierNode @operator, Node left, Node right, Range<Position> range) : OperatorNode(@operator, range)
{
	public Node Left { get; } = left;
	public Node Right { get; } = right;

	private static readonly Dictionary<string, int> Precedence = new()
	{
		{ Operators.Multiply, 3 }, { Operators.Divide, 3 }, { Operators.Modulo, 3 },
		{ Operators.Plus, 2 }, { Operators.Minus, 2 },
		{ Operators.Greater, 1 }, { Operators.Less, 1 }, { Operators.GreaterOrEqual, 1 }, { Operators.LessOrEqual, 1 },
		{ Operators.Equal, 0 }, { Operators.NotEqual, 0 }
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

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
