using ProgrammingLanguage.Application.Parsing;
using ProgrammingLanguage.Shared.Helpers;
using static System.Math;

namespace ProgrammingLanguage.Application.Evaluating;

internal class Runtime
{
	private readonly Module Module = new("@System");
	private readonly Structure Global;

	public Runtime()
	{
		Global = Module.RegisterType("@Global", typeof(Type), ~Position.Zero);

		ImportCore(~Position.Zero);
	}

	public void Evaluate(IEnumerable<Node> trees)
	{
		Evaluator evaluator = new(Module);
		foreach (Node tree in trees) tree.Accept(evaluator);
	}

	private void ImportCore(Range<Position> range)
	{
		Module.RegisterType("Type", typeof(Type), range);
		Module.RegisterType("Function", typeof(OperationContent), range);

		ImportNumber(range);
		ImportBoolean(range);
		ImportString(range);
		ImportMath(range);
		ImportInOut(range);
	}

	private void ImportNumber(Range<Position> range)
	{
		Structure typeNumber = Module.RegisterType("Number", typeof(double), range);

		typeNumber.RegisterOperation("+", ["Number", "Number"], "Number", NumberBinaryPlus, range);
		typeNumber.RegisterOperation("-", ["Number", "Number"], "Number", NumberBinaryMinus, range);
		typeNumber.RegisterOperation("*", ["Number", "Number"], "Number", NumberMultiplication, range);
		typeNumber.RegisterOperation("/", ["Number", "Number"], "Number", NumberDivision, range);

		typeNumber.RegisterOperation("+", ["Number"], "Number", NumberUnaryPlus, range);
		typeNumber.RegisterOperation("-", ["Number"], "Number", NumberUnaryMinus, range);

		typeNumber.RegisterOperation("=", ["Number", "Number"], "Boolean", NumberEqual, range);
		typeNumber.RegisterOperation("<", ["Number", "Number"], "Boolean", NumberLess, range);
		typeNumber.RegisterOperation("<=", ["Number", "Number"], "Boolean", NumberLessOrEqual, range);
		typeNumber.RegisterOperation(">", ["Number", "Number"], "Boolean", NumberGreater, range);
		typeNumber.RegisterOperation(">=", ["Number", "Number"], "Boolean", NumberGreaterOrEqual, range);
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
		Global.RegisterConstant("pi", "Number", PI, range);
		Global.RegisterConstant("e", "Number", E, range);
	}

	private void ImportInOut(Range<Position> range)
	{
		Global.RegisterOperation("write", ["Number"], "Number", Write, range);
		Global.RegisterOperation("write", ["Boolean"], "Number", Write, range);
		Global.RegisterOperation("write", ["String"], "Number", Write, range);
	}

	private static ValueNode NumberBinaryPlus(IEnumerable<ValueNode> args, Range<Position> range)
	{
		using IEnumerator<ValueNode> iterator = args.GetEnumerator();
		iterator.MoveNext();
		double left = iterator.Current.ValueAs<double>();
		iterator.MoveNext();
		double right = iterator.Current.ValueAs<double>();
		return new ValueNode("Number", left + right, range);
	}

	private static ValueNode NumberBinaryMinus(IEnumerable<ValueNode> args, Range<Position> range)
	{
		using IEnumerator<ValueNode> iterator = args.GetEnumerator();
		iterator.MoveNext();
		double left = iterator.Current.ValueAs<double>();
		iterator.MoveNext();
		double right = iterator.Current.ValueAs<double>();
		return new ValueNode("Number", left - right, range);
	}

	private static ValueNode NumberMultiplication(IEnumerable<ValueNode> args, Range<Position> range)
	{
		using IEnumerator<ValueNode> iterator = args.GetEnumerator();
		iterator.MoveNext();
		double left = iterator.Current.ValueAs<double>();
		iterator.MoveNext();
		double right = iterator.Current.ValueAs<double>();
		return new ValueNode("Number", left * right, range);
	}

	private static ValueNode NumberDivision(IEnumerable<ValueNode> args, Range<Position> range)
	{
		using IEnumerator<ValueNode> iterator = args.GetEnumerator();
		iterator.MoveNext();
		double left = iterator.Current.ValueAs<double>();
		iterator.MoveNext();
		double right = iterator.Current.ValueAs<double>();
		return new ValueNode("Number", left / right, range);
	}

	private static ValueNode NumberUnaryPlus(IEnumerable<ValueNode> args, Range<Position> range)
	{
		using IEnumerator<ValueNode> iterator = args.GetEnumerator();
		iterator.MoveNext();
		double target = iterator.Current.ValueAs<double>();
		return new ValueNode("Number", target, range);
	}
	private static ValueNode NumberUnaryMinus(IEnumerable<ValueNode> args, Range<Position> range)
	{
		using IEnumerator<ValueNode> iterator = args.GetEnumerator();
		iterator.MoveNext();
		double target = iterator.Current.ValueAs<double>();
		return new ValueNode("Number", -target, range);
	}

	private static ValueNode NumberEqual(IEnumerable<ValueNode> args, Range<Position> range)
	{
		using IEnumerator<ValueNode> iterator = args.GetEnumerator();
		iterator.MoveNext();
		double left = iterator.Current.ValueAs<double>();
		iterator.MoveNext();
		double right = iterator.Current.ValueAs<double>();
		return new ValueNode("Boolean", left == right, range);
	}

	private static ValueNode NumberLess(IEnumerable<ValueNode> args, Range<Position> range)
	{
		using IEnumerator<ValueNode> iterator = args.GetEnumerator();
		iterator.MoveNext();
		double left = iterator.Current.ValueAs<double>();
		iterator.MoveNext();
		double right = iterator.Current.ValueAs<double>();
		return new ValueNode("Boolean", left < right, range);
	}

	private static ValueNode NumberLessOrEqual(IEnumerable<ValueNode> args, Range<Position> range)
	{
		using IEnumerator<ValueNode> iterator = args.GetEnumerator();
		iterator.MoveNext();
		double left = iterator.Current.ValueAs<double>();
		iterator.MoveNext();
		double right = iterator.Current.ValueAs<double>();
		return new ValueNode("Boolean", left <= right, range);
	}

	private static ValueNode NumberGreater(IEnumerable<ValueNode> args, Range<Position> range)
	{
		using IEnumerator<ValueNode> iterator = args.GetEnumerator();
		iterator.MoveNext();
		double left = iterator.Current.ValueAs<double>();
		iterator.MoveNext();
		double right = iterator.Current.ValueAs<double>();
		return new ValueNode("Boolean", left > right, range);
	}

	private static ValueNode NumberGreaterOrEqual(IEnumerable<ValueNode> args, Range<Position> range)
	{
		using IEnumerator<ValueNode> iterator = args.GetEnumerator();
		iterator.MoveNext();
		double left = iterator.Current.ValueAs<double>();
		iterator.MoveNext();
		double right = iterator.Current.ValueAs<double>();
		return new ValueNode("Boolean", left >= right, range);
	}

	private static ValueNode Write(IEnumerable<ValueNode> args, Range<Position> range)
	{
		using IEnumerator<ValueNode> iterator = args.GetEnumerator();
		iterator.MoveNext();
		Console.WriteLine(iterator.Current.ToString());
		return ValueNode.NullableAt("Number", range);
	}
}