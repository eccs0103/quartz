using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Semantic;

public class ImmutableAssignmentIssue(string name, Range<Position> range) : SemanticIssue($"Cannot assign to immutable target '{name}'", range)
{
	public string Name { get; } = name;
}
