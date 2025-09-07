using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Parsing;

public abstract class OperatorNode(string @operator, Range<Position> range) : Node(range)
{
	public readonly string Operator = @operator;
}
