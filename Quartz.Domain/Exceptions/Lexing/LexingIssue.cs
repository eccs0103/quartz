using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Lexing;

public abstract class LexingIssue(string message, Range<Position> range) : Issue(message, range);
