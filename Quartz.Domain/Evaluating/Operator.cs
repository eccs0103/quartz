using Quartz.Domain.Exceptions;
using Quartz.Shared.Helpers;

namespace Quartz.Domain.Evaluating;

public class Operator(string name, Scope location) : Symbol(name)
{
	public override void Assign(Value value, Range<Position> range)
	{
		throw new NotMutableIssue($"Operator '{Name}'", range);
	}

	public void RegisterOperation(Operation operation, Range<Position> range)
	{
		location.Register(operation.Name, operation, range);
	}

	public Operation? TryReadOperation(IEnumerable<string> parameters)
	{
		string name = Mangle(parameters);
		if (location.TryRead(name, out Symbol? symbol) && symbol is Operation operation) return operation;
		return location.Find<Operation>((overload) =>
		{
			using IEnumerator<string> expected = overload.Parameters.GetEnumerator();
			using IEnumerator<string> provided = parameters.GetEnumerator();
			while (expected.MoveNext())
			{ 
				if (!provided.MoveNext()) return false;
				if (!TypeHelper.IsCompatible(expected.Current, provided.Current)) return false;
			}
			return !provided.MoveNext();
		});
	}

	// TODO ReadOperation

	public static string Mangle(IEnumerable<string> tags)
	{
		return $"({string.Join(", ", tags)})";
	}
}
