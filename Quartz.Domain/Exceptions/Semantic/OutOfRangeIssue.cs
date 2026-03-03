using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Semantic;

public class OutOfRangeIssue(string item, int begin, int end, Range<Position> range) : SemanticIssue($"{item} is out of range [{begin} - {end})", range)
{
	public OutOfRangeIssue(int index, int length, Range<Position> range) : this($"Index {index}", 0, length, range)
	{
	}
}
