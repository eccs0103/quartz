using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Parsing;

public abstract class ParsingIssue(string message, Range<Position> range) : Issue(message, range);
