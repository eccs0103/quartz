using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Instance(string tag, object value)
{
	public static Instance Null { get; } = new("Null", Empty.Instance);

	public string Tag { get; } = tag;

	public T ValueAs<T>()
	{
		if (value is T result) return result;
		string tag = value.GetType().Name;
		throw new InvalidCastException($"Unable to convert '{value}' from {tag} to {typeof(T).Name}");
	}

	public object Value => ValueAs<object>();

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
