using ProgrammingLanguage.Application.Parsing;
using ProgrammingLanguage.Shared.Helpers;
using static System.Math;

namespace ProgrammingLanguage.Application.Evaluating;

internal class Evaluator
{
	private readonly Module Module = new();
	private readonly ValueResolver Valuator;
	private readonly IdentifierResolver Nominator;

	public Evaluator()
	{
		Valuator = new(Module);
		Nominator = new(Module);

		Valuator.Nominator = Nominator;
		Nominator.Valuator = Valuator;

		ImportCore(~Position.Zero);
	}

	private void ImportCore(Range<Position> range)
	{
		ImportType(range);
		ImportNumber(range);
		ImportBoolean(range);
		ImportString(range);
		ImportMath(range);

		Module.RegisterOperation("write", Write, range);
	}

	private void ImportType(Range<Position> range)
	{
		Module.RegisterType("Type", typeof(Type), range);
	}

	private void ImportNumber(Range<Position> range)
	{
		Structure typeNumber = Module.RegisterType("Number", typeof(double), range);

		typeNumber.RegisterOperation("+", Plus, range);

		typeNumber.RegisterOperation("-", Minus, range);

		typeNumber.RegisterOperation("*", Multiplication, range);

		typeNumber.RegisterOperation("/", Division, range);
	}

	private ValueNode Plus(IdentifierNode nodeOperand, IEnumerable<Node> arguments, Range<Position> range)
	{
		IEnumerator<Node> enumerator = arguments.GetEnumerator();
		if (!enumerator.MoveNext()) throw new ArgumentException($"No overload for '{nodeOperand.Name}' that takes 0 arguments");
		ValueNode nodeLeft = enumerator.Current.Accept(Valuator);
		if (nodeLeft.Tag != "Number") throw new Exception($"Cannot apply '{nodeOperand.Name}' to types '{nodeLeft.Tag}'");
		if (!enumerator.MoveNext()) return UnaryPlus(nodeLeft, range);
		ValueNode nodeRight = enumerator.Current.Accept(Valuator);
		if (nodeRight.Tag != "Number") throw new Exception($"Cannot apply '{nodeOperand.Name}' to types '{nodeLeft.Tag}' and '{nodeRight.Tag}'");
		return BinaryPlus(nodeLeft, nodeRight, range);
	}

	private ValueNode BinaryPlus(ValueNode nodeLeft, ValueNode nodeRight, Range<Position> range)
	{
		double left = nodeLeft.ValueAs<double>();
		double right = nodeRight.ValueAs<double>();
		return new ValueNode("Number", left + right, range);
	}

	private ValueNode UnaryPlus(ValueNode nodeTarget, Range<Position> range)
	{
		double target = nodeTarget.ValueAs<double>();
		return new ValueNode("Number", +target, range);
	}

	private ValueNode Minus(IdentifierNode nodeOperand, IEnumerable<Node> arguments, Range<Position> range)
	{
		IEnumerator<Node> enumerator = arguments.GetEnumerator();
		if (!enumerator.MoveNext()) throw new ArgumentException($"No overload for '{nodeOperand.Name}' that takes 0 arguments");
		ValueNode nodeLeft = enumerator.Current.Accept(Valuator);
		if (nodeLeft.Tag != "Number") throw new Exception($"Cannot apply '{nodeOperand.Name}' to types '{nodeLeft.Tag}'");
		if (!enumerator.MoveNext()) return UnaryMinus(nodeLeft, range);
		ValueNode nodeRight = enumerator.Current.Accept(Valuator);
		if (nodeRight.Tag != "Number") throw new Exception($"Cannot apply '{nodeOperand.Name}' to types '{nodeLeft.Tag}' and '{nodeRight.Tag}'");
		return BinaryMinus(nodeLeft, nodeRight, range);
	}

	private ValueNode BinaryMinus(ValueNode nodeLeft, ValueNode nodeRight, Range<Position> range)
	{
		double left = nodeLeft.ValueAs<double>();
		double right = nodeRight.ValueAs<double>();
		return new ValueNode("Number", left - right, range);
	}

	private ValueNode UnaryMinus(ValueNode nodeTarget, Range<Position> range)
	{
		double target = nodeTarget.ValueAs<double>();
		return new ValueNode("Number", -target, range);
	}

	private ValueNode Multiplication(IdentifierNode nodeOperand, IEnumerable<Node> arguments, Range<Position> range)
	{
		IEnumerator<Node> enumerator = arguments.GetEnumerator();
		if (!enumerator.MoveNext()) throw new ArgumentException($"No overload for '{nodeOperand.Name}' that takes 0 arguments");
		ValueNode nodeLeft = enumerator.Current.Accept(Valuator);
		if (nodeLeft.Tag != "Number") throw new Exception($"Cannot apply '{nodeOperand.Name}' to types '{nodeLeft.Tag}'");
		if (!enumerator.MoveNext()) throw new ArgumentException($"No overload for '{nodeOperand.Name}' that takes 1 arguments");
		ValueNode nodeRight = enumerator.Current.Accept(Valuator);
		if (nodeRight.Tag != "Number") throw new Exception($"Cannot apply '{nodeOperand.Name}' to types '{nodeLeft.Tag}' and '{nodeRight.Tag}'");
		return BinaryMultiplication(nodeLeft, nodeRight, range);
	}

	private ValueNode BinaryMultiplication(ValueNode nodeLeft, ValueNode nodeRight, Range<Position> range)
	{
		double left = nodeLeft.ValueAs<double>();
		double right = nodeRight.ValueAs<double>();
		return new ValueNode("Number", left * right, range);
	}

	private ValueNode Division(IdentifierNode nodeOperand, IEnumerable<Node> arguments, Range<Position> range)
	{
		IEnumerator<Node> enumerator = arguments.GetEnumerator();
		if (!enumerator.MoveNext()) throw new ArgumentException($"No overload for '{nodeOperand.Name}' that takes 0 arguments");
		ValueNode nodeLeft = enumerator.Current.Accept(Valuator);
		if (nodeLeft.Tag != "Number") throw new Exception($"Cannot apply '{nodeOperand.Name}' to types '{nodeLeft.Tag}'");
		if (!enumerator.MoveNext()) throw new ArgumentException($"No overload for '{nodeOperand.Name}' that takes 1 arguments");
		ValueNode nodeRight = enumerator.Current.Accept(Valuator);
		if (nodeRight.Tag != "Number") throw new Exception($"Cannot apply '{nodeOperand.Name}' to types '{nodeLeft.Tag}' and '{nodeRight.Tag}'");
		return BinaryDivision(nodeLeft, nodeRight, range);
	}

	private ValueNode BinaryDivision(ValueNode nodeLeft, ValueNode nodeRight, Range<Position> range)
	{
		double left = nodeLeft.ValueAs<double>();
		double right = nodeRight.ValueAs<double>();
		return new ValueNode("Number", left / right, range);
	}

	private void ImportBoolean(Range<Position> range)
	{
		Module.RegisterType("Boolean", typeof(bool), range);
	}

	private void ImportString(Range<Position> range)
	{
		Module.RegisterType("String", typeof(string), range);
	}

	private void ImportMath(Range<Position> range)
	{
		Module.RegisterConstant("Number", "pi", PI, range);

		Module.RegisterConstant("Number", "e", E, range);
	}

	private ValueNode Write(IdentifierNode nodeOperand, IEnumerable<Node> arguments, Range<Position> range)
	{
		foreach (Node nodeArgument in arguments)
		{
			Console.WriteLine(nodeArgument.Accept(Valuator).Value);
		}
		return ValueNode.NullableAt("Number", range);
	}

	public void Evaluate(IEnumerable<Node> trees)
	{
		foreach (Node tree in trees) tree.Accept(Valuator);
	}
}
