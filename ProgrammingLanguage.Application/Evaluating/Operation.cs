using System.Runtime.CompilerServices;
using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Application.Parsing;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Evaluating;

internal delegate ValueNode OperationContent(IEnumerable<ValueNode> arguments, Range<Position> range);

internal class Operation(string name, IEnumerable<string> parameters, string result, OperationContent function) : Property(name, "Operation", function)
{
	public OperationContent Content => Unsafe.As<OperationContent>(Value);
	public readonly IEnumerable<string> Parameters = parameters;
	public readonly string Result = result;

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
		ValueNode result = Content.Invoke(results, range);
		if (result.Tag != Result) throw new TypeMismatchIssue(result.Tag, Result, range);
		return result;
	}
}
