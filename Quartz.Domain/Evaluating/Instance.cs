using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Instance(string tag, object? value, Range<Position> range, Scope location)
{
	public string Tag { get; } = tag;
	public Range<Position> RangePosition => range;

	public T ValueAs<T>()
	{
		if (value is T result) return result;
		if (value == null && default(T) == null) return default!;
		string tag = value?.GetType().Name ?? "Null";
		throw new InvalidCastException($"Unable to convert '{value}' from {tag} to {typeof(T).Name}");
	}

	public Instance Unwrap()
	{
		object? value = ValueAs<object?>();
		if (value == null) return new Instance("Null", null, RangePosition, location);
		string tag = Tag.EndsWith('?') ? Tag[..^1] : Tag;
		if (tag == Tag) return this;
		return new Instance(tag, value, RangePosition, location);
	}

	public Instance RunOperation(string name, IEnumerable<Instance> arguments)
	{
		IEnumerable<string> types = arguments.Select(arg => arg.Tag).Prepend(Tag);
		Symbol symbol = location.Read(Tag, RangePosition);
		if (symbol is not Class type) throw new NotExistIssue($"Type '{Tag}' in {location}", RangePosition);
		Operation operation = type.ReadOperation(name, types, RangePosition);
		IEnumerable<Instance> args = arguments.Prepend(this);
		Instance result = operation.Invoke(args, location, RangePosition);
		return result;
	}
}
