using ProgrammingLanguage.Shared.Helpers;

namespace ProgrammingLanguage.Application.Exceptions;

public class TypeMismatchIssue(string from, string to, Range<Position> range) : Issue($"Cannot implicitly convert type '{from}' to '{to}'", range)
{
}
