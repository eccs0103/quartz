using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Semantic;

public class SymbolAlreadyDeclaredIssue(string name, Range<Position> range) : SemanticIssue($"Symbol '{name}' is already declared", range)
{
	public string Name { get; } = name;
}
