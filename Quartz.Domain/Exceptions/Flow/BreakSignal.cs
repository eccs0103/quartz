using Quartz.Shared.Helpers;

namespace Quartz.Domain.Exceptions.Flow;

public class BreakSignal(Range<Position> range) : Issue("Unhandled break", range)
{
}
