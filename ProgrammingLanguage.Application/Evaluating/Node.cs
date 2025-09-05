using ProgrammingLanguage.Shared.Exceptions;

namespace ProgrammingLanguage.Application.Evaluating;

public abstract partial class Node
{
	protected static T PreventEvaluation<T>(in Node node) where T : Node
	{
		throw new Issue($"Unable to evaluate {typeof(T).Name} from {node.GetType().Name}", node.RangePosition.Begin);
	}
	protected static T Cast<T>(in Node node) where T : Node
	{
		return node as T ?? PreventEvaluation<T>(node);
	}
	protected static bool IsCompatible<T, N>() where T : Node where N : Node
	{
		return typeof(T) == typeof(N);
	}
	public virtual T Evaluate<T>(in Interpreter interpreter) where T : Node
	{
		return PreventEvaluation<T>(this);
	}
}
