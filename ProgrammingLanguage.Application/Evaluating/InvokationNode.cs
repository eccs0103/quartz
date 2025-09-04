using System;
using System.Text;
using ProgrammingLanguage.Shared.Exceptions;

namespace ProgrammingLanguage.Application.Evaluating;

public partial class InvokationNode : Node
{
	public override T Evaluate<T>(in Interpreter interpreter)
	{
		if (IsCompatible<T, ValueNode>())
		{
			switch (Target.Name)
			{
			case "Write":
			{
				StringBuilder builder = new();
				foreach (Node argument in Arguments)
				{
					if (builder.Length > 0) builder.Append('\n');
					builder.Append(argument.Evaluate<ValueNode>(interpreter).GetValue<double>());
				}
				Console.WriteLine(builder.ToString());
				return Cast<T>(new ValueNode(null, RangePosition));
			}
			default: throw new Issue($"Function '{Target.Name}' does not exist", RangePosition.Begin);
			}
		}
		return PreventEvaluation<T>(this);
	}
}

