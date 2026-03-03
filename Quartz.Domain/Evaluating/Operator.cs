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
			bool isMatch = true;
			using IEnumerator<string> iterator = parameters.GetEnumerator();
			foreach (string expected in overload.Parameters)
			{
				if (iterator.MoveNext() && TypeHelper.IsCompatible(expected, iterator.Current, Location)) continue;
				isMatch = false;
				break;
			}
			if (!isMatch) continue;
			if (iterator.MoveNext()) continue;
			operation = overload;
			return true;
		}
		operation = null;
		return false;
	}
}
