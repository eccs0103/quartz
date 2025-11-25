namespace Quartz.Domain.Evaluating;

internal static class TypeHelper
{
	public static bool IsCompatible(string target, string value)
	{
		if (target == value) return true;
		if (!target.EndsWith('?')) return false;
		if (value == "Null") return true;
		return target[..^1] == value;
	}

	// Внутри класса TypeHelper
	public static bool IsOptional(string tag)
	{
		// 1. Тип "Null" сам по себе является пустотой, инициализация не требуется.
		if (tag == "Null") return true;

		// 2. Nullable типы (с вопросом) подразумевают, что там может быть пусто.
		if (tag.EndsWith("?")) return true;

		// 3. Все остальные (строгие типы) требуют явного значения.
		return false;
	}
}
