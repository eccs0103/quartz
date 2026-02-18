using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Semantic;

public class NotCallableIssue(string target, Range<Position> range) : SemanticIssue($"'{target}' is not callable", range)
{
	public string Target { get; } = target;
}
