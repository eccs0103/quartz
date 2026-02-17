using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Variable(string name, string typeTag, Value value, bool mutable)
{
	public string Name { get; } = name;
	public string TypeTag { get; } = typeTag;
	public Value Value { get; private set; } = value;
	public bool Mutable { get; } = mutable;

	public void Assign(Value newValue, Scope scope, Range<Position> range)
	{
		if (!Mutable) throw new NotMutableIssue($"Variable '{Name}'", range);
		// Use the declared type tag, not the current value's tag (though they should perform similarly)
		if (!TypeHelper.IsCompatible(TypeTag, newValue.Tag, scope)) throw new TypeMismatchIssue(TypeTag, newValue.Tag, range);
		Value = newValue;
	}
}
