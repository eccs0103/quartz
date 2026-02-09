namespace Quartz.Domain.Evaluating;

public static class TypeHelper
{
	public static bool IsCompatible(string target, string value)
	{
		if (target == "Any") return true;
		if (target == value) return true;
		if (!target.EndsWith('?')) return false;
		if (value == "Null") return true;
		return target.AsSpan(0, target.Length - 1).SequenceEqual(value.AsSpan());
	}

	public static bool IsOptional(string tag)
	{
		if (tag == "Null") return true;
		if (tag.EndsWith('?')) return true;
		return false;
	}

	public static Instance Unwrap(Instance instance)
	{
		if (instance.ValueAs<object>() is Null) return new Instance("Null", Null.Instance, instance.Location);
		string tag = instance.Tag.EndsWith('?') ? instance.Tag[..^1] : instance.Tag;
		if (tag == instance.Tag) return instance;
		return new Instance(tag, instance.ValueAs<object>(), instance.Location);
	}
}
