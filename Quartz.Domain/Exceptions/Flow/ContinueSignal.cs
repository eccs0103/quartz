using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Flow;

public class ContinueSignal(Range<Position> range) : Issue("Unhandled continue", range)
{
}
