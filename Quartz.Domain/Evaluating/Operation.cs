using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Operation(string name, IEnumerable<string> parameters, string result, Func<Value[], Scope, Range<Position>, Value> content, Scope location) : Symbol(name)
{
	public IEnumerable<string> Parameters { get; } = parameters;
	public string Result { get; } = result;

	public override void Assign(Value value, Range<Position> range)
	{
		throw new NotMutableIssue($"Operation '{Name}'", range);
	}

	public Value Invoke(IEnumerable<Value> arguments, Scope scope, Range<Position> range)
	{
		List<Value> results = [];
		using IEnumerator<Value> iterator = arguments.GetEnumerator();
		foreach (string expected in Parameters)
		{
			if (!iterator.MoveNext()) throw new NoOverloadIssue(Name, Convert.ToByte(results.Count), range);
			Value provided = iterator.Current;
			if (!TypeHelper.IsCompatible(expected, provided.Tag)) throw new TypeMismatchIssue(expected, provided.Tag, range);
			results.Add(provided);
		}
		Value result = content.Invoke([.. results], scope, range);
		if (!TypeHelper.IsCompatible(Result, result.Tag)) throw new TypeMismatchIssue(Result, result.Tag, range);
		return result;
	}
}
