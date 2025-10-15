using ProgrammingLanguage.Application.Exceptions;
using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Evaluating;

internal class Module(string name)
{
	private readonly Scope Scope = new(name);

	public Structure RegisterType(string name, Type equivalent, Range<Position> range)
	{
		Structure type = new(name, equivalent, Scope);
		Scope.Register(name, type, range);
		return type;
	}

	public Structure ReadType(string name, Range<Position> range)
	{
		Property property = Scope.Read(name, range);
		if (property is not Structure type) throw new NotExistIssue($"Identifier '{name}' in {Scope}", range);
		return type;
	}
}
