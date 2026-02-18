using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Semantic;

public class InvalidBinaryOperandIssue(string @operator, string left, string right, Range<Position> range) : SemanticIssue($"Operator '{@operator}' cannot be applied to operands of type '{left}' and '{right}'", range)
{
	public string Operator { get; } = @operator;
	public string Left { get; } = left;
	public string Right { get; } = right;
}
