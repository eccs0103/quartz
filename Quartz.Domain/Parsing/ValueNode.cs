using System.Globalization;
using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Parsing;

public class ValueNode(string tag, object? value, Range<Position> range) : Node(range)
{
	public string Tag { get; } = tag;
	public object? Value { get; } = value;

	public override string ToString()
	{
		if (Tag == "String") return $"\"{Value}\"";
		if (Tag == "Boolean" && Value is bool boolean) return boolean ? "true" : "false";
		if (Value is double number) return number.ToString(CultureInfo.InvariantCulture);
		return Value?.ToString() ?? "null";
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
