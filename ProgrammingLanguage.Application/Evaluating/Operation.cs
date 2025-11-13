using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Application.Parsing;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Evaluating;

internal delegate ValueNode OperationContent(Scope location, ValueNode[] arguments, Range<Position> range);

internal class Operation(string name, IEnumerable<string> parameters, string result, OperationContent content, Scope location) : Symbol(name)
{
	public IEnumerable<string> Parameters { get; } = parameters;
	public string Result { get; } = result;

	public ValueNode Invoke(IEnumerable<ValueNode> arguments, Range<Position> range)
	{
		List<ValueNode> results = [];
		using IEnumerator<ValueNode> iterator = arguments.GetEnumerator();
		foreach (string expected in Parameters)
		{
			if (!iterator.MoveNext()) throw new NoOverloadIssue(Name, Convert.ToByte(results.Count), range);
			ValueNode provided = iterator.Current;
			if (provided.Tag != expected) throw new TypeMismatchIssue(expected, provided.Tag, provided.RangePosition);
			results.Add(provided);
		}
		Scope scope = location.GetSubscope("Call");
		ValueNode result = content.Invoke(scope, [.. results], range);
		if (result.Tag != Result) throw new TypeMismatchIssue(result.Tag, Result, range);
		return result;
	}
}
