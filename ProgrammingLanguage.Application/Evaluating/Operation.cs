using System.Runtime.CompilerServices;
using ProgrammingLanguage.Application.Abstractions;
using ProgrammingLanguage.Application.Parsing;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Evaluating;

internal class Operation(string name, Function function) : Datum("Function", name, function)
{
	public Node Invoke(IdentifierNode nodeOperand, IEnumerable<Node> arguments, Range<Position> range)
	{
		return Unsafe.As<Function>(Value)!.Invoke(nodeOperand, arguments, range);
	}
}
