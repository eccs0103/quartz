using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions;

public abstract class Issue(string message, Range<Position> range) : Exception($"{message} at {range.Begin}")
{
	public Range<Position> Range { get; } = range;
}
