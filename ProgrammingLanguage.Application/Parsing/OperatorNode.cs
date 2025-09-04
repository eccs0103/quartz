using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Parsing;

public abstract partial class OperatorNode(in string @operator, in Range<Position> range) : Node(range)
{
	public readonly string Operator = @operator;
}

