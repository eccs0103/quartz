namespace Quartz.Domain.Evaluating;

internal static class TypeHelper
{
	public static bool IsCompatible(string target, string value)
	{
		if (target == "Any") return true;
		if (target == value) return true;
		if (!target.EndsWith('?')) return false;
		if (value == "Null") return true;
		return target[..^1] == value;
	}

	public static bool IsOptional(string tag)
	{
		if (tag == "Null") return true;
		if (tag.EndsWith('?')) return true;
		return false;
	}
}
