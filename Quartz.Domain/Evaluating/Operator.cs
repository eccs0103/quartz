using System.Diagnostics.CodeAnalysis;
using Quartz.Domain.Exceptions;
using Quartz.Shared.Extensions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Operator(string name, Scope location)
{
	public string Name { get; } = name;

	public bool TryRegisterOperation(Operation operation)
	{
		return location.TryRegister(operation.Name, new Value<Operation>(TypeConstants.Function, operation));
	}

	public void RegisterOperation(Operation operation, Range<Position> range)
	{
		if (!TryRegisterOperation(operation)) throw new AlreadyExistsIssue($"Operation '{operation.Name}' in {location}", range);
	}

	public bool TryReadOperation(IEnumerable<string> parameters, [NotNullWhen(true)] out Operation? operation)
	{
		if (location.TryRead(Mangler.Parameters(parameters), out operation)) return true;
		foreach (Operation overload in location.Scan<Operation>())
		{
			bool isMatch = true;
			using IEnumerator<string> iterator = parameters.GetEnumerator();
			foreach (string expected in overload.Parameters)
			{
				if (iterator.MoveNext() && TypeHelper.IsCompatible(expected, iterator.Current, location)) continue;
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

	public Operation ReadOperation(IEnumerable<string> parameters, Range<Position> range)
	{
		if (!TryReadOperation(parameters, out Operation? operation)) throw new NotExistIssue($"Operation '{Name}{Mangler.Parameters(parameters)}' in {location}", range);
		return operation;
	}
}
