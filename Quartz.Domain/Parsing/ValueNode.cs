using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class ValueNode(string tag, object? value, Range<Position> range) : Node(range)
{
	public string Tag { get; } = tag;
	public object? Value { get; } = value;

	public override string ToString()
	{
		return $"{Value ?? "null"}";
	}

	public static ValueNode NullAt(Range<Position> range)
	{
		return new ValueNode("Null", null, range);
	}

	public override T Accept<T>(IAstVisitor<T> visitor, Scope location)
	{
		return visitor.Visit(location, this);
	}
}
