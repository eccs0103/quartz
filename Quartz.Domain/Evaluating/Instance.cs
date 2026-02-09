using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Instance(string tag, object value, Scope location)
{
	public string Tag { get; } = tag;
	public Scope Location => location;

	public T ValueAs<T>()
	{
		if (value is T result) return result;
		string tag = value.GetType().Name;
		throw new InvalidCastException($"Unable to convert '{value}' from {tag} to {typeof(T).Name}");
	}

	public Instance RunOperation(string name, IEnumerable<Instance> arguments, Range<Position> range)
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
