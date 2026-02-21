using System.Text;
using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;
using static Quartz.Shared.Constants;


namespace Quartz.Domain.Parsing;

public class BlockNode(IEnumerable<Node> statements, Range<Position> range) : Node(range)
{
	public IEnumerable<Node> Statements { get; } = statements;

	public override string ToString()
	{
		const string indent = "  ";
		StringBuilder builder = new();
		builder.AppendLine(Brackets.OpenBrace);
		foreach (Node statement in Statements)
		{
			string @string = $"{statement}";
			builder.Append(indent);
			builder.AppendLine(@string.Replace(Environment.NewLine, $"{Environment.NewLine}{indent}"));
		}
		builder.Append(Brackets.CloseBrace);
		return builder.ToString();
	}

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
