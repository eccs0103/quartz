using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Parsing;

public partial class UnaryOperatorNode(in string @operator, in Node target, in Range<Position> range) : OperatorNode(@operator, range)
{
	public readonly Node Target = target;
	public override string ToString()
	{
		return $"{Operator}({Target})";
	}
}

