using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Variable(string name, string tag, Value value, bool mutable)
{
	public string Name { get; } = name;
	public string Tag { get; } = tag;
	public Value Value { get; private set; } = value;
	public bool Mutable { get; } = mutable;

	public void Assign(Value newValue, Scope scope, Range<Position> range)
	{
		if (!Mutable) throw new NotMutableIssue($"Variable '{Name}'", range);
		if (!TypeHelper.IsCompatible(Tag, newValue.Tag, scope)) throw new TypeMismatchIssue(Tag, newValue.Tag, range);
		Value = newValue;
	}
}
