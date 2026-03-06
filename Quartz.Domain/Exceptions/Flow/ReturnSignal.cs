using Quartz.Domain.Evaluating;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Flow;

public class ReturnSignal(Range<Position> range, Value value) : Issue("Unhandled return", range)
{
	public Value Value { get; } = value;
}
