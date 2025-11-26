using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions;

public class InitializationRequiredIssue(string identifier, string tag, Range<Position> range) : Issue($"Initialization required for {identifier} {tag}", range)
{
}
