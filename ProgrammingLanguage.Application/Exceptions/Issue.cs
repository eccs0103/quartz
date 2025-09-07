using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Exceptions;

public class Issue(string message, Position position) : Exception($"{message} at {position}")
{
	public Position Position { get; } = position;
}
