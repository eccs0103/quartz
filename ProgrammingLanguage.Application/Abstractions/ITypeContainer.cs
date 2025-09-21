using ProgrammingLanguage.Application.Evaluating;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Abstractions;

internal interface ITypeContainer
{
	public Structure RegisterType(string name, Type equivalent, Range<Position> range);
	public Structure ReadType(string name, Range<Position> range);
}
