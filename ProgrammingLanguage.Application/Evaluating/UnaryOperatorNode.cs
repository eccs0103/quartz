using ProgrammingLanguage.Application.Parsing;

namespace ProgrammingLanguage.Application.Evaluating;

public partial class UnaryOperatorNode : OperatorNode
{
	public override T Evaluate<T>(in Interpreter interpreter)
	{
		if (IsCompatible<T, ValueNode>())
		{
			switch (Operator)
			{
			case "+":
			case "-":
			{
				return Cast<T>(new BinaryOperatorNode(Operator, new ValueNode(0, RangePosition), Target, RangePosition).Evaluate<ValueNode>(interpreter));
			}
			case "data":
			{
				return Cast<T>(Evaluate<IdentifierNode>(interpreter).Evaluate<ValueNode>(interpreter));
			}
			case "import":
			{
				string address = Target.Evaluate<ValueNode>(interpreter).GetValue<string>();
				string input = Fetch(address) ?? throw new Issue($"Executable APL file in '{address}' doesn't exist", RangePosition.Begin);
				interpreter.Run(input);
				return Cast<T>(new ValueNode(null, RangePosition));
			}
			default: throw new Issue($"Unidentified '{Operator}' operator", RangePosition.Begin);
			}
		}
		if (IsCompatible<T, IdentifierNode>())
		{
			switch (Operator)
			{
			case "data":
			{
				IdentifierNode identifier = Target.Evaluate<IdentifierNode>(interpreter);
				if (!interpreter.Memory.TryAdd(identifier.Name, new Datum(null, new(true)))) throw new Issue($"Identifier '{identifier.Name}' already exists", RangePosition.Begin);
				return Cast<T>(identifier);
			}
			default: throw new Issue($"Unidentified '{Operator}' operator", RangePosition.Begin);
			}
		}
		return PreventEvaluation<T>(this);
	}
}
