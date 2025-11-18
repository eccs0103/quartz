using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class BreakStatementNode(Range<Position> range) : Node(range)
{
	public override string ToString()
	{
		throw new NotImplementedException();
	}

	public override T Accept<T>(IAstVisitor<T> visitor, Scope location)
	{
		return visitor.Visit(location, this);
	}
}
