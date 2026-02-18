using Quartz.Domain.Exceptions.Semantic;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public abstract class Value(string tag, object content)
{
	public static object Empty { get; } = new();
	public static Value Null { get; } = new Value<object>("Null", Empty);
	public string Tag { get; } = tag;
	public object Content { get; } = content;

	public Value<T> As<T>()
		where T : notnull
	{
		if (this is Value<T> value) return value;
		if (Content is T content) return new Value<T>(Tag, content);
		throw new InvalidCastException($"Unable to convert '{Content}' from {Content.GetType().Name} to {typeof(T).Name}");
	}

	public Value RunOperation(string name, IEnumerable<Value> arguments, Scope location, Range<Position> range)
	{
		IEnumerable<string> types = arguments.Select(arg => arg.Tag).Prepend(Tag);
		if (!location.TryRead(Tag, out Class? type)) throw new SymbolNotFoundIssue(Tag, "Type class", range);
		if (!type.TryReadOperation(name, types, out Operation? operation)) throw new NoMatchingOverloadIssue(name, types, range);
		arguments = arguments.Prepend(this);
		Value result = operation.Invoke(arguments, location, range);
		return result;
	}
}

public class Value<T>(string tag, T content) : Value(tag, content)
	where T : notnull
{
	public new T Content { get; } = content;
}
