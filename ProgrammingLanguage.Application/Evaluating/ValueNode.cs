namespace ProgrammingLanguage.Application.Evaluating;

public partial class ValueNode : Node
{
	public override T Evaluate<T>(in Interpreter interpreter)
	{
		if (IsCompatible<T, ValueNode>()) return Cast<T>(this);
		return PreventEvaluation<T>(this);
	}
}
