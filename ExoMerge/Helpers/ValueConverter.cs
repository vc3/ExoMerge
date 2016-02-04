using System;
using System.Collections;
using System.Linq;

namespace ExoMerge.Helpers
{
	internal static class ValueConverter
	{
		/// <summary>
		/// Determines if the given type is a true numeric type, not simply convertable to a number.
		/// </summary>
		private static bool IsNumericZero(object value, double tolerance = 0.000001)
		{
			if (value is int)
				return (int)value == 0;
			if (value is long)
				return (long)value == 0;
			if (value is short)
				return (short)value == 0;
			if (value is decimal)
				return (decimal)value == 0;
			if (value is double)
				return Math.Abs((double)value) < tolerance;
			if (value is float)
				return Math.Abs((float)value) < tolerance;
			if (value is uint)
				return (uint)value == 0;
			if (value is ulong)
				return (ulong)value == 0;
			if (value is ushort)
				return (ushort)value == 0;

			return false;
		}

		/// <summary>
		/// Converts the given value to a boolean using basic "truthiness" rules.
		/// </summary>
		public static bool ToBoolean(object value)
		{
			if (value is bool)
				return (bool)value;

			// Treat an empty list as false.
			var items = value as IEnumerable;
			if (items != null && !(value is string))
				return items.Cast<object>().Any();

			// Treat empty string or whitespace as false.
			var str = value as string;
			if (str != null)
				return str.Trim().Length > 0;

			// Treat '0' (numeric zero) as false.
			if (IsNumericZero(value))
				return false;

			// Treat null as false.
			return value != null;
		}
	}
}
