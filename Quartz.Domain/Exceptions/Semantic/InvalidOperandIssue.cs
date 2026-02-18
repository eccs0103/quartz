using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Semantic;

public class InvalidOperandIssue(string @operator, string type, Range<Position> range) : SemanticIssue($"Operator '{@operator}' cannot be applied to operand of type '{type}'", range)
{
	public string Operator { get; } = @operator;
	public string Type { get; } = type;
}
