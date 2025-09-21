using ProgrammingLanguage.Application.Evaluating;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Abstractions;

internal interface IOperationContainer
{
	public Operation RegisterOperation(string name, Function function, Range<Position> range);
	public Operation ReadOperation(string name, Range<Position> range);
}
