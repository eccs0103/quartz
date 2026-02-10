using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class GenericNode(IdentifierNode type, IEnumerable<IdentifierNode> generics, Range<Position> range) : IdentifierNode($"{type.Name}<{string.Join(", ", generics.Select(g => g.Name))}>", range) // Inherit IdentifierNode to stay compatible with DeclarationNode
{
	public IdentifierNode Target { get; } = type;
	public IEnumerable<IdentifierNode> Generics { get; } = generics;

	public override string ToString()
	{
		return $"{Target}<{string.Join(", ", Generics)}>";
	}
}
