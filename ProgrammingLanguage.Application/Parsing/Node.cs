using ProgrammingLanguage.Application.Evaluating;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Parsing;

internal abstract class Node(Range<Position> range)
{
	public readonly Range<Position> RangePosition = range;

	public abstract T Accept<T>(IAstVisitor<T> visitor, Scope location);
}
