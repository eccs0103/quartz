using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Parsing;

public class AssignmentTargetIssue(Range<Position> range) : ParsingIssue("Assignment target must be a variable, field or property", range);
