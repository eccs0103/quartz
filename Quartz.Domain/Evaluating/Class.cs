using System.Diagnostics.CodeAnalysis;
using static Quartz.Domain.Definitions;

namespace Quartz.Domain.Evaluating;

public class Class(string name, Scope location, string? @base) : Container(name, location)
{
	private bool HierarchyInitialized { get; set; } = false;
	private (int Depth, int[] Display) Hierarchy { get; set; } = (0, []);
	private static int NextTypeId = 0;
	public int Id { get; } = Interlocked.Increment(ref NextTypeId);
	public int Depth => EnsureHierarchy().Depth;
	public int[] Display => EnsureHierarchy().Display;

	private (int Depth, int[] Display) EnsureHierarchy()
	{
		if (HierarchyInitialized) return Hierarchy;
		HierarchyInitialized = true;
		if (TryGetBase(out Class? @base))
		{
			int depth = @base.Depth + 1;
			int length = Math.Max(8, depth + 1);
			int[] display = new int[length];
			Array.Copy(@base.Display, display, @base.Display.Length);
			display[depth] = Id;
			return Hierarchy = (depth, display);
		}
		int[] @default = new int[8];
		@default[0] = Id;
		return Hierarchy = (0, @default);
	}

	private bool TryGetBase([NotNullWhen(true)] out Class? type)
	{
		if (@base != null) return Location.TryRead(@base, out type);
		type = null;
		return false;
	}

	public bool TryRegisterOperator(Operator @operator)
	{
		return Location.TryRegister(@operator.Name, Types.Function, new Value<Operator>(Types.Function, @operator), false);
	}

	public bool TryReadOperator(string name, [NotNullWhen(true)] out Operator? @operator)
	{
		if (Location.TryRead(name, out @operator, false)) return true;
		if (TryGetBase(out Class? baseClass)) return baseClass.TryReadOperator(name, out @operator);
		@operator = null;
		return false;
	}

	public bool TryRegisterOperation(string name, Operation operation)
	{
		if (!TryReadOperator(name, out Operator? @operator))
		{
			@operator = new Operator(name, Location.GetSubscope(name));
			if (!TryRegisterOperator(@operator)) return false;
		}
		return @operator.TryRegisterOperation(operation);
	}

	public bool TryReadOperation(string name, IEnumerable<string> parameters, [NotNullWhen(true)] out Operation? operation)
	{
		if (TryReadOperator(name, out Operator? @operator) && @operator.TryReadOperation(parameters, out operation)) return true;
		if (TryGetBase(out Class? baseClass)) return baseClass.TryReadOperation(name, parameters, out operation);
		operation = null;
		return false;
	}

	public bool TryRegisterVariable(string name, string tag, Value value)
	{
		return Location.TryRegister(name, tag, value, true);
	}

	public bool TryRegisterConstant(string name, string tag, Value value)
	{
		return Location.TryRegister(name, tag, value, false);
	}

	public bool TryReadProperty(string name, [NotNullWhen(true)] out Variable? variable)
	{
		if (Location.TryRead(name, out variable, false)) return true;
		if (TryGetBase(out Class? baseClass)) return baseClass.TryReadProperty(name, out variable);
		variable = null;
		return false;
	}
}
