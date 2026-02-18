using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Semantic;

public class VariableNotInitializedIssue(string name, Range<Position> range) : SemanticIssue($"Variable '{name}' must be initialized", range)
{
	public string Name { get; } = name;
}
