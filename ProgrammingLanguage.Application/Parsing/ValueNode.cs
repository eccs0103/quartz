using ProgrammingLanguage.Application.Abstractions;
using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Parsing;

public class ValueNode(object? value, Range<Position> range) : Node(range)
{
	public readonly object? Value = value;

	public override string ToString()
	{
		return $"{Value ?? "null"}";
	}

	public T ValueAs<T>()
	{
		if (Value is T result) return result;
		string from = Value == null ? "Null" : Value.GetType().Name;
		throw new Issue($"Unable to convert from {from} to {typeof(T).Name}", RangePosition.Begin);
	}

	public static ValueNode NullAt(Range<Position> range)
	{
		return new ValueNode(null, range);
	}

	public override T Accept<T>(IEvaluatorVisitor<T> visitor)
	{
		return visitor.Visit(this);
	}
}
