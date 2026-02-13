using System.Text;
using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class BlockNode(IEnumerable<Node> statements, Range<Position> range) : Node(range)
{
	public IEnumerable<Node> Statements { get; } = statements;

	public override string ToString()
	{
		const string indent = "  ";
		StringBuilder builder = new();
		builder.AppendLine("{");
		foreach (Node statement in Statements)
		{
			string @string = $"{statement}";
			builder.Append(indent);
			builder.AppendLine(@string.Replace("\n", $"\n{indent}"));
		}
		builder.Append('}');
		return builder.ToString();
	}

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
