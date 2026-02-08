using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

internal delegate Instance OperationContent(Instance[] arguments, Scope location, Range<Position> range);
