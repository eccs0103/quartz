using Quartz.Domain.Exceptions;
using Quartz.Domain.Parsing;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

internal class Datum(string name, string tag, object value, bool mutable) : Symbol(name)
{
	public string Tag { get; } = tag;
	public bool Mutable { get; } = mutable;
	public object Value { get; private set; } = value;

	public override void Assign(ValueNode value, Range<Position> range)
	{
		if (!Mutable) throw new NotMutableIssue($"Datum '{Name}'", range);
		if (Tag != value.Tag) throw new TypeMismatchIssue(Tag, value.Tag, value.RangePosition);
		Value = value.Value!;
	}
}
