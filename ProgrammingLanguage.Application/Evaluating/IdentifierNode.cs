using System;
using ProgrammingLanguage.Application.Parsing;
using ProgrammingLanguage.Shared.Exceptions;

namespace ProgrammingLanguage.Application.Evaluating;

public partial class IdentifierNode : Node
{
	public override T Evaluate<T>(in Evalutor evalutor) 
	{
		if (IsCompatible<T, ValueNode>())
		{
			if (!evalutor.Memory.TryGetValue(Name, out Datum? datul)) throw new Issue($"Identifier '{Name}' does not exist", RangePosition.Begin);
			return Cast<T>(new ValueNode(datul.Value, RangePosition));
		}
		if (IsCompatible<T, IdentifierNode>()) return Cast<T>(this);
		return PreventEvaluation<T>(this);
	}
}

