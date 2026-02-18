using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Semantic;

public class NoMatchingOverloadIssue(string target, IEnumerable<string> parameters, Range<Position> range) : SemanticIssue($"No overload for '{target}' matches arguments ({string.Join(", ", parameters)})", range)
{
	public string Target { get; } = target;
	public IEnumerable<string> Parameters { get; } = parameters;
}
