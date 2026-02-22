using Quartz.Domain.Exceptions;
using static Quartz.Domain.Definitions;

namespace Quartz.Domain.Evaluating;

public static class TypeHelper
{
	public static bool IsCompatible(string target, string value, Scope scope)
	{
		if (target == Types.Any) return true;
		if (target == value) return true;
		if (Mangler.IsNullable(target, out string? inner))
		{
			if (value == Types.Null) return true;
			return IsCompatible(inner, value, scope);
		}
		if (scope.TryRead(target, out Class? typeTarget) && scope.TryRead(value, out Class? typeValue))
		{
			// TODO: Implement inheritance check here when available on Class
			// For now, assume no inheritance beyond implicit checks above
		}
		return false;
	}

	public static bool IsOptional(string tag)
	{
		return tag == Types.Null || Mangler.IsNullable(tag, out _);
	}

	public static Value Unwrap(Value value)
	{
		if (value.Content == Value.Empty) return Value.Null;
		string tag = UnwrapTag(value.Tag);
		if (tag == value.Tag) return value;
		return new Value<object>(tag, value.Content);
	}

	private static string UnwrapTag(string tag)
	{
		if (Mangler.IsNullable(tag, out string? inner)) return inner;
		return tag;
	}
}
