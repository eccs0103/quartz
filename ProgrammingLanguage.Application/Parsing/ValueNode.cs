using ProgrammingLanguage.Shared.Exceptions;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Parsing;

public partial class ValueNode(in object? value, in Range<Position> range) : Node(range)
{
	private readonly object? Value = value;
	public T GetValue<T>()
	{
		if (Value is T result) return result;
		string from = Value == null ? "Null" : Value.GetType().Name;
		throw new Issue($"Unable to convert from {from} to {typeof(T).Name}", RangePosition.Begin);
	}
	public override string ToString()
	{
		return $"{Value}";
	}
}
