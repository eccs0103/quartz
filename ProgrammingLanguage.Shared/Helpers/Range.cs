namespace ProgrammingLanguage.Shared.Helpers;

public class Range<T>(in T begin, in T end)
{
	public T Begin { get; } = begin;
	public T End { get; } = end;
	public override string ToString()
	{
		return $"from {Begin} to {End}";
	}
}
