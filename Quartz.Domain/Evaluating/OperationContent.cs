using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public delegate Instance OperationContent(Instance[] arguments, Scope location, Range<Position> range);
