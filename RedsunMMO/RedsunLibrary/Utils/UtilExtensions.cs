using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace RedsunLibrary.Utils
{
	public static class UtilExtensions
	{
#if NET5_0_OR_GREATER
		public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value)
		{
			return string.IsNullOrEmpty(value);
		}

		public static bool IsNullOrEmpty<T>([NotNullWhen(false)] this IEnumerable<T> values)
		{
			if (values == null || values.Count() == 0)
			{
				return true;
			}
			return false;
		}

		public static bool IsNull<T>([NotNullWhen(false)] this T value)
		{
			if (value == null)
			{
				return true;
			}
			return false;
		}
#else
		public static bool IsNullOrEmpty(this string value)
		{
			return string.IsNullOrEmpty(value);
		}

		public static bool IsNullOrEmpty<T>(this IEnumerable<T> values)
		{
			if (values == null || values.Count() == 0)
			{
				return true;
			}
			return false;
		}

		public static bool IsNull<T>(this T value)
		{
			if (value == null)
			{
				return true;
			}
			return false;
		}
#endif
	}
}
