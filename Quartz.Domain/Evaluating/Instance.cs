using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Instance(string tag, object value)
{
	public static object Empty { get; } = new();
	public static Instance Null { get; } = new Instance<object>("Null", Empty);

	public string Tag { get; } = tag;
	public object Value { get; } = value;

	public Instance<T> As<T>()
		where T : notnull
	{
		if (this is Instance<T> instance) return instance;
		if (Value is T value) return new Instance<T>(Tag, value);
		string tag = Value.GetType().Name;
		throw new InvalidCastException($"Unable to convert '{Value}' from {tag} to {typeof(T).Name}");
	}

	public Instance RunOperation(string name, IEnumerable<Instance> arguments, Scope location, Range<Position> range)
	{
		IEnumerable<string> types = arguments.Select(arg => arg.Tag).Prepend(Tag);
		Symbol symbol = location.Read(Tag, range);
		if (symbol is not Class type) throw new NotExistIssue($"Type '{Tag}' in {location}", range);
		Operation operation = type.ReadOperation(name, types, range);
		IEnumerable<Instance> args = arguments.Prepend(this);
		Instance result = operation.Invoke(args, location, range);
		return result;
	}
}

public class Instance<T>(string tag, T value) : Instance(tag, value)
	where T : notnull
{
	public new T Value { get; } = value;
}
