using System.Text;
using ProgrammingLanguage.Application.Abstractions;
using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Application.Parsing;
using static System.Math;

namespace ProgrammingLanguage.Application.Evaluating;

public class Evaluator : IEvaluatorVisitor<ValueNode>
{
	public readonly Dictionary<string, Datum> Database = new()
	{
		{ "pi", Datum.ConstantFrom(PI) },
		{ "e", Datum.ConstantFrom(E) },
	};
	// public readonly Dictionary<string, ValueNode> Functions = [];

	// private static void write(params object[] arguments)
	// {
	// 	StringBuilder builder = new();
	// 	foreach (object argument in arguments)
	// 	{
	// 		if (builder.Length > 0) builder.Append('\n');
	// 		builder.Append(argument.ToString());
	// 	}
	// 	Console.WriteLine(builder.ToString());
	// }

	public void Evaluate(IEnumerable<Node> trees)
	{
		foreach (Node tree in trees) tree.Accept(this);
	}

	public ValueNode Visit(ValueNode node)
	{
		return node;
	}

	public ValueNode Visit(IdentifierNode node)
	{
		if (!Database.TryGetValue(node.Name, out Datum? datum)) throw new Issue($"Identifier '{node.Name}' does not exist", node.RangePosition.Begin);
		return new ValueNode(datum.Value, node.RangePosition);
	}

	public ValueNode Visit(DeclarationNode node)
	{
		ValueNode value = node.Value.Accept(this);
		if (!Database.TryAdd(node.Identifier.Name, Datum.VariableFrom(value))) throw new Issue($"Identifier '{node.Identifier.Name}' already exists", node.RangePosition.Begin);
		return ValueNode.NullAt(node.RangePosition);
	}

	public ValueNode Visit(InvokationNode node)
	{
		switch (node.Target.Name)
		{
		case "write":
		{
			StringBuilder builder = new();
			foreach (Node argument in node.Arguments)
			{
				if (builder.Length > 0) builder.Append('\n');
				builder.Append(argument.Accept(this).GetValue<double>());
			}
			Console.WriteLine(builder.ToString());
			return ValueNode.NullAt(node.RangePosition);
		}
		default: throw new Issue($"Function '{node.Target.Name}' does not exist", node.RangePosition.Begin);
		}
	}

	public ValueNode Visit(UnaryOperatorNode node)
	{
		// case "import":
		// {
		// 	string address = Target.Evaluate<ValueNode>(interpreter).GetValue<string>();
		// 	string input = Fetch(address) ?? throw new Issue($"Executable APL file in '{address}' doesn't exist", RangePosition.Begin);
		// 	interpreter.Run(input);
		// 	return (new ValueNode(null, RangePosition));
		// }

		ValueNode target = node.Target.Accept(this);
		switch (node.Operator)
		{
		case "+": return new ValueNode(+target.GetValue<double>(), node.RangePosition);
		case "-": return new ValueNode(-target.GetValue<double>(), node.RangePosition);
		default: throw new Issue($"Unidentified '{node.Operator}' operator", node.RangePosition.Begin);
		}
	}

	public ValueNode Visit(BinaryOperatorNode node)
	{
		// switch (Operator)
		// {
		// case ":":
		// {
		// 	ValueNode right = Right.Evaluate<ValueNode>(interpreter);
		// 	IdentifierNode left = Left.Evaluate<IdentifierNode>(interpreter);
		// 	if (!interpreter.Memory.TryGetValue(left.Name, out Datum? datul)) throw new Issue($"Identifier '{left.Name}' does not exist", RangePosition.Begin);
		// 	if (!datul.Mutable) throw new Issue($"Identifier '{left.Name}' is non-mutable", RangePosition.Begin);
		// 	datul.Value = right.GetValue<object>();
		// 	return (left);
		// }
		// default: throw new Issue($"Unidentified '{Operator}' operator", RangePosition.Begin);
		// }

		ValueNode left = node.Left.Accept(this);
		ValueNode right = node.Right.Accept(this);

		switch (node.Operator)
		{
		case "+": return new ValueNode(left.GetValue<double>() + right.GetValue<double>(), node.RangePosition);
		case "-": return new ValueNode(left.GetValue<double>() - right.GetValue<double>(), node.RangePosition);
		case "*": return new ValueNode(left.GetValue<double>() * right.GetValue<double>(), node.RangePosition);
		case "/": return new ValueNode(left.GetValue<double>() / right.GetValue<double>(), node.RangePosition);
		default: throw new Issue($"Unidentified '{node.Operator}' operator", node.RangePosition.Begin);
		}
	}
}
