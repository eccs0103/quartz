using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public abstract class OperatorNode(IdentifierNode @operator, Range<Position> range) : Node(range)
{
	public IdentifierNode Operator { get; } = @operator;
}
