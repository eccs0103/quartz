using System;

namespace Quartz.Shared.Extensions;

public static class EnumerableExtensions
{
	public static string Mangle(this IEnumerable<object> items)
	{
		const string comma = ", ";
		return string.Join(comma, items);
	}
}
