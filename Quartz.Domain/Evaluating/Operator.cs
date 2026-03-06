using System.Diagnostics.CodeAnalysis;
using static Quartz.Domain.Definitions;

namespace Quartz.Domain.Evaluating;

public class Operator(string name, Scope location) : Container(name, location)
{
	public bool TryRegisterOperation(Operation operation)
	{
		return Location.TryRegister(operation.Name, Types.Function, new Value<Operation>(Types.Function, operation));
	}

	public bool TryReadOperation(IEnumerable<string> parameters, [NotNullWhen(true)] out Operation? operation)
	{
		if (Location.TryRead(Mangler.Parameters(parameters), out operation)) return true;
		foreach (Operation overload in Location.Scan<Operation>())
		{
			if (parameters.Count() != overload.Parameters.Count()) continue;
			if (parameters.Zip(overload.Parameters).Any(pair => !TypeHelper.IsCompatible(pair.Second, pair.First, Location))) continue;
			operation = overload;
			return true;
		}
		operation = null;
		return false;
	}
}
