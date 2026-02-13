namespace Quartz.Domain.Evaluating;

public static class TypeHelper
{
	public static bool IsCompatible(string target, string value)
	{
		if (target == "Any") return true;
		if (target == value) return true;
		bool isNullable = target.EndsWith('?') || (target.StartsWith("Nullable<") && target.EndsWith('>'));
		if (!isNullable) return false;
		if (value == "Null") return true;
		return IsCompatible(UnwrapTag(target), value);
	}

	public static bool IsOptional(string tag)
	{
		if (tag == "Null") return true;
		if (tag.EndsWith('?')) return true;
		if (tag.StartsWith("Nullable<") && tag.EndsWith('>')) return true;
		return false;
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
		if (tag.EndsWith('?')) return tag[..^1];
		if (tag.StartsWith("Nullable<") && tag.EndsWith('>')) return tag[9..^1];
		return tag;
	}
}
