using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Shared.Exceptions;

public class Issue(in string message, in Position position) : Exception($"{message} at {position}")
{
	public Position Position { get; } = position;
}
