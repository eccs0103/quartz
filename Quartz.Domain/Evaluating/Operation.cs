using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public delegate Value OperationContent(Value[] arguments, Scope scope, Range<Position> range);

public class Operation(string @operator, string name, IEnumerable<string> parameters, string result, OperationContent content, Scope location) : Symbol(name)
{
	public string Operator { get; } = @operator;
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
