using Quartz.Domain.Exceptions.Semantic;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public delegate Value OperationContent(Value[] arguments, Scope scope, Range<Position> range);

public class Operation(string name, IEnumerable<string> parameters, string result, OperationContent content, Scope location)
{
	public string Name { get; } = name;
	public IEnumerable<string> Parameters { get; } = parameters;
	public string Result { get; } = result;

	public Value Invoke(IEnumerable<Value> arguments, Scope scope, Range<Position> range)
	{
		List<Value> results = [];
		using IEnumerator<Value> iterator = arguments.GetEnumerator();
		foreach (string expected in Parameters)
		{
			if (!iterator.MoveNext()) throw new ArgumentCountIssue(Name, Parameters.Count(), results.Count, range);
			Value provided = iterator.Current;
			if (!TypeHelper.IsCompatible(expected, provided.Tag, scope)) throw new TypeMismatchIssue(expected, provided.Tag, range);
			results.Add(provided);
		}
		Value result = content.Invoke([.. results], scope, range);
		if (!TypeHelper.IsCompatible(Result, result.Tag, scope)) throw new TypeMismatchIssue(Result, result.Tag, range);
		return result;
	}
}
