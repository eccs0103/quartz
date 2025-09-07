namespace ProgrammingLanguage.Shared.Helpers;

public class Range<T>(in T begin, in T end)
{
	public readonly T Begin = begin;
	public readonly T End = end;

	public Range(Range<T> range) : this(range.Begin, range.End)
	{
	}

	public override string ToString()
	{
		return $"from {Begin} to {End}";
	}
}
