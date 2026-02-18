using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Semantic;

public abstract class SemanticIssue(string message, Range<Position> range) : Issue(message, range);
