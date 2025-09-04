using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Parsing;

public partial class IdentifierNode(in string name, in Range<Position> range) : Node(range)
{
	public readonly string Name = name;
	public override string ToString()
	{
		return $"{Name}";
	}
}

