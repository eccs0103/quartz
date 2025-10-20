using ProgrammingLanguage.Application.Parsing;
using ProgrammingLanguage.Shared.Helpers;
using static System.Math;

namespace ProgrammingLanguage.Application.Evaluating;

internal class Runtime
{
	private readonly Module Module = new("@System");

	public Runtime()
	{
		ImportCore(Module, ~Position.Zero);
	}

	public void Evaluate(IEnumerable<Node> trees)
	{
		Structure global = Module.ReadType("@Global", ~Position.Zero);
		Scope scope = new(global.Name);
		Evaluator evaluator = new();
		foreach (Node tree in trees) tree.Accept(evaluator, scope);
	}

	private static void ImportCore(Module module, Range<Position> range)
	{
		module.RegisterType("Type", typeof(Type), range);
		module.RegisterType("Function", typeof(OperationContent), range);
		ImportNumber(module, range);
		ImportBoolean(module, range);
		ImportString(module, range);
		ImportGlobal(module, range);
	}

	private static void ImportNumber(Module module, Range<Position> range)
	{
		Structure typeNumber = module.RegisterType("Number", typeof(double), range);

		typeNumber.RegisterOperation("+", ["Number", "Number"], "Number", NumberAdd, range);
		typeNumber.RegisterOperation("-", ["Number", "Number"], "Number", NumberSubtract, range);
		typeNumber.RegisterOperation("*", ["Number", "Number"], "Number", NumberMultiply, range);
		typeNumber.RegisterOperation("/", ["Number", "Number"], "Number", NumberDivide, range);

		typeNumber.RegisterOperation("+", ["Number"], "Number", NumberPositive, range);
		typeNumber.RegisterOperation("-", ["Number"], "Number", NumberNegate, range);

		typeNumber.RegisterOperation("=", ["Number", "Number"], "Boolean", NumberEquals, range);
		typeNumber.RegisterOperation("<", ["Number", "Number"], "Boolean", NumberLessThan, range);
		typeNumber.RegisterOperation("<=", ["Number", "Number"], "Boolean", NumberLessThanOrEqual, range);
		typeNumber.RegisterOperation(">", ["Number", "Number"], "Boolean", NumberGreaterThan, range);
		typeNumber.RegisterOperation(">=", ["Number", "Number"], "Boolean", NumberGreaterThanOrEqual, range);
	}

	private static ValueNode NumberAdd(ValueNode[] args, Range<Position> range)
	{
		double left = args[0].ValueAs<double>();
		double right = args[1].ValueAs<double>();
		return new ValueNode("Number", left + right, range);
	}

	private static ValueNode NumberSubtract(ValueNode[] args, Range<Position> range)
	{
		double left = args[0].ValueAs<double>();
		double right = args[1].ValueAs<double>();
		return new ValueNode("Number", left - right, range);
	}

	private static ValueNode NumberMultiply(ValueNode[] args, Range<Position> range)
	{
		double left = args[0].ValueAs<double>();
		double right = args[1].ValueAs<double>();
		return new ValueNode("Number", left * right, range);
	}

	private static ValueNode NumberDivide(ValueNode[] args, Range<Position> range)
	{
		double left = args[0].ValueAs<double>();
		double right = args[1].ValueAs<double>();
		return new ValueNode("Number", left / right, range);
	}

	private static ValueNode NumberPositive(ValueNode[] args, Range<Position> range)
	{
		double target = args[0].ValueAs<double>();
		return new ValueNode("Number", +target, range);
	}

	private static ValueNode NumberNegate(ValueNode[] args, Range<Position> range)
	{
		double target = args[0].ValueAs<double>();
		return new ValueNode("Number", -target, range);
	}

	private static ValueNode NumberEquals(ValueNode[] args, Range<Position> range)
	{
		double left = args[0].ValueAs<double>();
		double right = args[1].ValueAs<double>();
		return new ValueNode("Boolean", left == right, range);
	}

	private static ValueNode NumberLessThan(ValueNode[] args, Range<Position> range)
	{
		double left = args[0].ValueAs<double>();
		double right = args[1].ValueAs<double>();
		return new ValueNode("Boolean", left < right, range);
	}

	private static ValueNode NumberLessThanOrEqual(ValueNode[] args, Range<Position> range)
	{
		double left = args[0].ValueAs<double>();
		double right = args[1].ValueAs<double>();
		return new ValueNode("Boolean", left <= right, range);
	}

	private static ValueNode NumberGreaterThan(ValueNode[] args, Range<Position> range)
	{
		double left = args[0].ValueAs<double>();
		double right = args[1].ValueAs<double>();
		return new ValueNode("Boolean", left > right, range);
	}

	private static ValueNode NumberGreaterThanOrEqual(ValueNode[] args, Range<Position> range)
	{
		double left = args[0].ValueAs<double>();
		double right = args[1].ValueAs<double>();
		return new ValueNode("Boolean", left >= right, range);
	}

	private static void ImportBoolean(Module module, Range<Position> range)
	{
		module.RegisterType("Boolean", typeof(bool), range);
	}

	private static void ImportString(Module module, Range<Position> range)
	{
		module.RegisterType("String", typeof(string), range);
	}

	private static void ImportGlobal(Module module, Range<Position> range)
	{
		Structure typeGlobal = module.RegisterType("@Global", typeof(Type), range);

		typeGlobal.RegisterConstant("pi", "Number", PI, range);
		typeGlobal.RegisterConstant("e", "Number", E, range);

		typeGlobal.RegisterOperation("write", ["Number"], "Number", Write, range);
		typeGlobal.RegisterOperation("write", ["Boolean"], "Number", Write, range);
		typeGlobal.RegisterOperation("write", ["String"], "Number", Write, range);
	}

	private static ValueNode Write(ValueNode[] args, Range<Position> range)
	{
		Console.WriteLine(args[0].ToString());
		return ValueNode.NullableAt("Number", range);
	}
}