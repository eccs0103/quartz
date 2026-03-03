using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class TemplateNode(IdentifierNode target, IEnumerable<IdentifierNode> generics, Range<Position> range) : IdentifierNode(Mangler.Generics(target.Name, generics.Select(generic => generic.Name)), range)
{
	public IdentifierNode Target { get; } = target;
	public IEnumerable<IdentifierNode> Generics { get; } = generics;

	public override string ToString()
	{
		return Name;
	}

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
