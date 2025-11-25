using Quartz.Domain.Exceptions;
using Quartz.Domain.Parsing;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

internal class Operation(string name, IEnumerable<string> parameters, string result, OperationContent content, Scope location) : Symbol(name)
{
	public IEnumerable<string> Parameters { get; } = parameters;
	public string Result { get; } = result;

	public override void Assign(ValueNode value, Range<Position> range)
	{
		throw new NotMutableIssue($"Operation '{Name}'", range);
	}

	public ValueNode Invoke(IEnumerable<ValueNode> arguments, Scope scope, Range<Position> range)
	{
		List<ValueNode> results = [];
		using IEnumerator<ValueNode> iterator = arguments.GetEnumerator();
		foreach (string expected in Parameters)
		{
			if (!iterator.MoveNext()) throw new NoOverloadIssue(Name, Convert.ToByte(results.Count), range);
			ValueNode provided = iterator.Current;
			if (!TypeHelper.IsCompatible(expected, provided.Tag)) throw new TypeMismatchIssue(expected, provided.Tag, provided.RangePosition);
			results.Add(provided);
		}
		ValueNode result = content.Invoke([.. results], scope, range);
		if (!TypeHelper.IsCompatible(Result, result.Tag)) throw new TypeMismatchIssue(Result, result.Tag, range);
		return result;
	}
}
