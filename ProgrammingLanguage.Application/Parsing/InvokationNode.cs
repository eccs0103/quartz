using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Parsing;

public partial class InvokationNode(in IdentifierNode target, in Node[] arguments, in Range<Position> range) : Node(range)
{
	public readonly IdentifierNode Target = target;
	public readonly Node[] Arguments = arguments;
	public override string ToString()
	{
		return $"{Target}({string.Join<Node>(", ", Arguments)})";
	}
}
