using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Datum(string name, string tag, Value value, bool mutable) : Symbol(name)
{
	public string Tag { get; } = tag;
	public bool Mutable { get; } = mutable;
	public Value Value { get; private set; } = value;

	public override void Assign(Value value, Scope scope, Range<Position> range)
	{
		if (!Mutable) throw new NotMutableIssue($"Datum '{Name}'", range);
		if (!TypeHelper.IsCompatible(Tag, value.Tag, scope)) throw new TypeMismatchIssue(Tag, value.Tag, range);
		Value = value;
	}
}
