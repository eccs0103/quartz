using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions;

public class NoOverloadIssue(string name, byte count, Range<Position> range) : Issue($"No overload for '{name}' that takes {count} arguments", range)
{
}
