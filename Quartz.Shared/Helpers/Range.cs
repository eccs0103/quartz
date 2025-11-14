namespace Quartz.Shared.Helpers;

public class Range<T>(in T begin, in T end)
{
	public T Begin { get; } = begin;
	public T End { get; } = end;

	public Range(Range<T> range) : this(range.Begin, range.End)
	{
	}

	public override string ToString()
	{
		return $"from {Begin} to {End}";
	}

	public static Range<T> operator >>(Range<T> from, Range<T> to)
	{
		return new(from.Begin, to.End);
	}
}
