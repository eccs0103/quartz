using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Parsing;

public abstract partial class Node(in Range<Position> range)
{
	public readonly Range<Position> RangePosition = range;
}

