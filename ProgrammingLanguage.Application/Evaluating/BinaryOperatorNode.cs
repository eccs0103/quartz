using System;
using ProgrammingLanguage.Application.Parsing;

namespace ProgrammingLanguage.Application.Evaluating;

public partial class BinaryOperatorNode : OperatorNode
{
	public override T Evaluate<T>(in Interpreter interpreter)
	{
		if (IsCompatible<T, ValueNode>())
		{
			switch (Operator)
			{
				case "+":
					{
						double left = Left.Evaluate<ValueNode>(interpreter).GetValue<double>();
						double right = Right.Evaluate<ValueNode>(interpreter).GetValue<double>();
						return Cast<T>(new ValueNode(left + right, RangePosition));
					}
				case "-":
					{
						double left = Left.Evaluate<ValueNode>(interpreter).GetValue<double>();
						double right = Right.Evaluate<ValueNode>(interpreter).GetValue<double>();
						return Cast<T>(new ValueNode(left - right, RangePosition));
					}
				case "*":
					{
						double left = Left.Evaluate<ValueNode>(interpreter).GetValue<double>();
						double right = Right.Evaluate<ValueNode>(interpreter).GetValue<double>();
						return Cast<T>(new ValueNode(left * right, RangePosition));
					}
				case "/":
					{
						double left = Left.Evaluate<ValueNode>(interpreter).GetValue<double>();
						double right = Right.Evaluate<ValueNode>(interpreter).GetValue<double>();
						return Cast<T>(new ValueNode(left / right, RangePosition));
					}
				case ":":
					{
						return Cast<T>(Evaluate<IdentifierNode>(interpreter).Evaluate<ValueNode>(interpreter));
					}
				default: throw new Issue($"Unidentified '{Operator}' operator", RangePosition.Begin);
			}
		}
		if (IsCompatible<T, IdentifierNode>())
		{
			switch (Operator)
			{
				case ":":
					{
						ValueNode right = Right.Evaluate<ValueNode>(interpreter);
						IdentifierNode left = Left.Evaluate<IdentifierNode>(interpreter);
						if (!interpreter.Memory.TryGetValue(left.Name, out Datum? datul)) throw new Issue($"Identifier '{left.Name}' does not exist", RangePosition.Begin);
						if (!datul.Mutable) throw new Issue($"Identifier '{left.Name}' is non-mutable", RangePosition.Begin);
						datul.Value = right.GetValue<object>();
						return Cast<T>(left);
					}
				default: throw new Issue($"Unidentified '{Operator}' operator", RangePosition.Begin);
			}
		}
		return PreventEvaluation<T>(this);
	}
}

