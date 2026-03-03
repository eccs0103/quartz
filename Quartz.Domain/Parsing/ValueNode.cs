using System.Globalization;
using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;
using static Quartz.Domain.Definitions;

namespace Quartz.Domain.Parsing;

public class ValueNode(string tag, object? value, Range<Position> range) : Node(range)
{
	public string Tag { get; } = tag;
	public object? Value { get; } = value;

	public override string ToString()
	{
		if (Value is string text) return $"\"{Value}\"";
		if (Value is char character) return $"'{Value}'";
		if (Value is bool boolean) return boolean ? Keywords.True : Keywords.False;
		if (Value is double number) return number.ToString(CultureInfo.InvariantCulture);
		return Value?.ToString() ?? Keywords.Null;
	}

	public override T Accept<T>(IEvaluator<T> evaluator, Scope location)
	{
		return evaluator.Evaluate(location, this);
	}
}
