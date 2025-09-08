using ProgrammingLanguage.Application.Abstractions;
using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Application.Parsing;
using static System.Math;

namespace ProgrammingLanguage.Application.Evaluating;

public class Evaluator : IEvaluatorVisitor<ValueNode>
{
	private readonly Registry Memory = new();

	public Evaluator()
	{
		Memory.TryDeclareType("Type", typeof(Type), out _);
		Memory.TryDeclareType("Number", typeof(double), out _);
		Memory.TryDeclareType("Boolean", typeof(bool), out _);
		Memory.TryDeclareType("String", typeof(string), out _);
		Memory.TryDeclareConstant("Number", "pi", PI, out _);
		Memory.TryDeclareConstant("Number", "e", E, out _);
	}

	/* private static void write(double first, double second)
	{
		Console.WriteLine($"{first}\n{second}");
	} */

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
		if (!Memory.TryRead(node.Name, out object? value)) throw new Issue($"Identifier '{node.Name}' does not exist", node.RangePosition.Begin);
		return new ValueNode(value, node.RangePosition);
	}

	public ValueNode Visit(DeclarationNode node)
	{
		ValueNode value = node.Value.Accept(this);
		if (!Memory.TryDeclareVariable("Number", node.Identifier.Name, value.Value, out _)) throw new Issue($"Identifier '{node.Identifier.Name}' already exists", node.RangePosition.Begin);
		return ValueNode.NullAt(node.RangePosition);
	}

	public ValueNode Visit(InvokationNode node)
	{
		throw new NotImplementedException();
		// if (!Functions.TryGetValue(node.Target.Name, out Function? function)) throw new Issue($"Function '{node.Target.Name}' does not exist", node.RangePosition.Begin);
		// IEnumerable<ValueNode> arguments = node.Arguments.Select(argument => argument.Accept(this));
		// return function.Invoke(arguments, node.RangePosition.Begin);
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
		case "+": return new ValueNode(+target.ValueAs<double>(), node.RangePosition);
		case "-": return new ValueNode(-target.ValueAs<double>(), node.RangePosition);
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
		case "+": return new ValueNode(left.ValueAs<double>() + right.ValueAs<double>(), node.RangePosition);
		case "-": return new ValueNode(left.ValueAs<double>() - right.ValueAs<double>(), node.RangePosition);
		case "*": return new ValueNode(left.ValueAs<double>() * right.ValueAs<double>(), node.RangePosition);
		case "/": return new ValueNode(left.ValueAs<double>() / right.ValueAs<double>(), node.RangePosition);
		default: throw new Issue($"Unidentified '{node.Operator}' operator", node.RangePosition.Begin);
		}
	}
}
