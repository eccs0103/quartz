using ProgrammingLanguage.Application.Parsing;
using ProgrammingLanguage.Shared.Helpers;
using static System.Math;

namespace ProgrammingLanguage.Application.Evaluating;

internal class Runtime
{
	private static readonly Scope System = new("@System");
	private static readonly Scope Workspace = System.GetSubscope("@Developer");
	private static readonly Evaluator Evaluator = new();

	public Runtime()
	{
		ImportSystem(~Position.Zero);
	}

	public void Evaluate(IEnumerable<Node> trees)
	{
		foreach (Node tree in trees) tree.Accept(Evaluator, Workspace);
	}

	#region System
	private static void ImportSystem(Range<Position> range)
	{
		Module module = new("@System", System);

		ImportType(module, range);
		ImportFunction(module, range);
		ImportNumber(module, range);
		ImportBoolean(module, range);
		ImportString(module, range);
		ImportDeveloper(module, range);
	}
	#region Type
	private static void ImportType(Module module, Range<Position> range)
	{
		module.RegisterClass("Type", range);
	}
	#endregion
	#region Function
	private static void ImportFunction(Module module, Range<Position> range)
	{
		module.RegisterClass("Function", range);
	}
	#endregion
	#region Number
	private static void ImportNumber(Module module, Range<Position> range)
	{
		Class type = module.RegisterClass("Number", range);

		Operator opAdd = type.RegisterOperator("+", range);
		opAdd.RegisterOperation(["Number"], "Number", NumberPositive, range);
		opAdd.RegisterOperation(["Number", "Number"], "Number", NumberAdd, range);

		Operator opSub = type.RegisterOperator("-", range);
		opSub.RegisterOperation(["Number"], "Number", NumberNegate, range);
		opSub.RegisterOperation(["Number", "Number"], "Number", NumberSubtract, range);

		Operator opMul = type.RegisterOperator("*", range);
		opMul.RegisterOperation(["Number", "Number"], "Number", NumberMultiply, range);

		Operator opDiv = type.RegisterOperator("/", range);
		opDiv.RegisterOperation(["Number", "Number"], "Number", NumberDivide, range);

		Operator opEq = type.RegisterOperator("=", range);
		opEq.RegisterOperation(["Number", "Number"], "Boolean", NumberEquals, range);

		Operator opLess = type.RegisterOperator("<", range);
		opLess.RegisterOperation(["Number", "Number"], "Boolean", NumberLessThan, range);

		Operator opLessEq = type.RegisterOperator("<=", range);
		opLessEq.RegisterOperation(["Number", "Number"], "Boolean", NumberLessThanOrEqual, range);

		Operator opGreat = type.RegisterOperator(">", range);
		opGreat.RegisterOperation(["Number", "Number"], "Boolean", NumberGreaterThan, range);

		Operator opGreatEq = type.RegisterOperator(">=", range);
		opGreatEq.RegisterOperation(["Number", "Number"], "Boolean", NumberGreaterThanOrEqual, range);
	}

	private static ValueNode NumberPositive(Scope location, ValueNode[] args, Range<Position> range)
	{
		double target = args[0].ValueAs<double>();
		return new ValueNode("Number", +target, range);
	}

	private static ValueNode NumberAdd(Scope location, ValueNode[] args, Range<Position> range)
	{
		double left = args[0].ValueAs<double>();
		double right = args[1].ValueAs<double>();
		return new ValueNode("Number", left + right, range);
	}

	private static ValueNode NumberNegate(Scope location, ValueNode[] args, Range<Position> range)
	{
		double target = args[0].ValueAs<double>();
		return new ValueNode("Number", -target, range);
	}

	private static ValueNode NumberSubtract(Scope location, ValueNode[] args, Range<Position> range)
	{
		double left = args[0].ValueAs<double>();
		double right = args[1].ValueAs<double>();
		return new ValueNode("Number", left - right, range);
	}

	private static ValueNode NumberMultiply(Scope location, ValueNode[] args, Range<Position> range)
	{
		double left = args[0].ValueAs<double>();
		double right = args[1].ValueAs<double>();
		return new ValueNode("Number", left * right, range);
	}

	private static ValueNode NumberDivide(Scope location, ValueNode[] args, Range<Position> range)
	{
		double left = args[0].ValueAs<double>();
		double right = args[1].ValueAs<double>();
		return new ValueNode("Number", left / right, range);
	}

	private static ValueNode NumberEquals(Scope location, ValueNode[] args, Range<Position> range)
	{
		double left = args[0].ValueAs<double>();
		double right = args[1].ValueAs<double>();
		return new ValueNode("Boolean", left == right, range);
	}

	private static ValueNode NumberLessThan(Scope location, ValueNode[] args, Range<Position> range)
	{
		double left = args[0].ValueAs<double>();
		double right = args[1].ValueAs<double>();
		return new ValueNode("Boolean", left < right, range);
	}

	private static ValueNode NumberLessThanOrEqual(Scope location, ValueNode[] args, Range<Position> range)
	{
		double left = args[0].ValueAs<double>();
		double right = args[1].ValueAs<double>();
		return new ValueNode("Boolean", left <= right, range);
	}

	private static ValueNode NumberGreaterThan(Scope location, ValueNode[] args, Range<Position> range)
	{
		double left = args[0].ValueAs<double>();
		double right = args[1].ValueAs<double>();
		return new ValueNode("Boolean", left > right, range);
	}

	private static ValueNode NumberGreaterThanOrEqual(Scope location, ValueNode[] args, Range<Position> range)
	{
		double left = args[0].ValueAs<double>();
		double right = args[1].ValueAs<double>();
		return new ValueNode("Boolean", left >= right, range);
	}
	#endregion
	#region Boolean
	private static void ImportBoolean(Module module, Range<Position> range)
	{
		module.RegisterClass("Boolean", range);
	}
	#endregion
	#region String
	private static void ImportString(Module module, Range<Position> range)
	{
		module.RegisterClass("String", range);
	}
	#endregion
	#region Developer
	private static void ImportDeveloper(Module module, Range<Position> range)
	{
		Class typeDeveloper = new("@Developer", Workspace);

		typeDeveloper.RegisterConstant("pi", "Number", PI, range);
		typeDeveloper.RegisterConstant("e", "Number", E, range);

		Operator opWrite = typeDeveloper.RegisterOperator("write", range);
		opWrite.RegisterOperation(["Number"], "Number", Write, range);
		opWrite.RegisterOperation(["Boolean"], "Number", Write, range);
		opWrite.RegisterOperation(["String"], "Number", Write, range);
	}

	private static ValueNode Write(Scope location, ValueNode[] args, Range<Position> range)
	{
		Console.WriteLine(args[0].ToString());
		return ValueNode.NullableAt("Number", range);
	}
	#endregion
	#endregion
}
