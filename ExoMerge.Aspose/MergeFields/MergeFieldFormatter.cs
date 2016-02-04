using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ExoMerge.Analysis;

namespace ExoMerge.Aspose.MergeFields
{
	public class MergeFieldFormatter
	{
		private static readonly string[] FormatSwitchKeys = { "*", "#", "@" };

		private static readonly IOptionParser SwitchParser = new MergeFieldSwitchParser(FormatSwitchKeys) { SkipOverUnknownSwitches = true };

		private static readonly Regex AmPmExpr = new Regex("(a/p)|(A/P)|(am/pm)|(AM/PM)", RegexOptions.Compiled);

		private static readonly Regex ElapsedExpr = new Regex("(\\[hh?\\])|(\\[mm?\\])|(\\[ss?\\])", RegexOptions.Compiled);

		private static readonly Regex MillisecondExpr = new Regex("\\.(0+)", RegexOptions.Compiled);

		private static readonly Regex FiveCharMonthExpr = new Regex("(?<!M)MMMMM(?!M)", RegexOptions.Compiled);

		private static readonly Regex HourMinuteExpr = new Regex("(?:h|H):(M+)", RegexOptions.Compiled);

		private static readonly Regex MinuteSecondExpr = new Regex("(M+):s", RegexOptions.Compiled);

		private static readonly Regex MinuteToMonthExpr = new Regex("(?<!a|p)m", RegexOptions.Compiled);

		private static readonly Regex MinutesExpr = new Regex("M*", RegexOptions.Compiled);

		private static readonly int[] MonthOrdinals = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

		private static readonly string[] MonthAbbreviations = MonthOrdinals.Select(i => new DateTime(2011, i, 1).ToString("MMM")).ToArray();

		private const string RandomMarker = "!@#$";

		/// <summary>
		/// Extract the format switches from the given expression.
		/// </summary>
		public static string ExtractFormats(string expression, out KeyValuePair<string, string>[] formats)
		{
			return SwitchParser.ExtractOptions(expression, out formats);
		}

		/// <summary>
		/// Apply the given format switches to the given value and return the result.
		/// </summary>
		public static string ApplyFormats(object value, KeyValuePair<string, string>[] switches)
		{
			if (value == null)
				return string.Empty;

			if (switches == null || switches.Length == 0)
				return Convert.ToString(value);

			foreach (var swtch in switches)
			{
				if (swtch.Key == "*")
				{
					// https://support.office.com/en-us/article/Insert-and-format-field-codes-in-Word-2010-7e9ea3b4-83ec-4203-9e66-4efc027f2cf3#bm7

					throw new NotImplementedException("Merge field format switch '*' is not implemented.");
				}
				else if (swtch.Key == "#")
				{
					// https://support.office.com/en-us/article/Insert-and-format-field-codes-in-Word-2010-7e9ea3b4-83ec-4203-9e66-4efc027f2cf3#bm8

					object numericValue;

					if (!TryGetNumeric(value, out numericValue))
						throw new Exception(string.Format("Cannot use the numeric format switch '\\# {0}' on an object of type {1}.", swtch.Value, value.GetType().Name));

					value = ApplyFormat(numericValue, MergeFieldSwitchParser.RemoveSurroundingQuotes(swtch.Value));
				}
				else if (swtch.Key == "@")
				{
					// https://support.office.com/en-us/article/Insert-and-format-field-codes-in-Word-2010-7e9ea3b4-83ec-4203-9e66-4efc027f2cf3#bm9

					DateTime? dateTimeValue;

					if (!TryGetDateTime(value, out dateTimeValue) || dateTimeValue == null)
						throw new Exception(string.Format("Cannot use the date/time format switch '\\@ {0}' on an object of type {1}.", swtch.Value, value.GetType().Name));

					value = ApplyDateTimeFormat(dateTimeValue.Value, MergeFieldSwitchParser.RemoveSurroundingQuotes(swtch.Value));
				}
			}

			return (string)value;
		}

		/// <summary>
		/// Attempts to cast the given value as a numeric type if it isn't already numeric.
		/// </summary>
		/// <remarks>
		/// http://stackoverflow.com/questions/9809340/how-to-check-if-isnumeric/9809405#9809405
		/// </remarks>
		private static bool TryGetNumeric(object value, out object result)
		{
			if (value is Int16 || value is Int32 || value is Int64 || value is Decimal || value is Single || value is Double)
			{
				result = value;
				return true;
			}

			if (value is string)
			{
				int intValue;
				long longValue;
				double doubleValue;

				if (int.TryParse((string)value, out intValue))
				{
					result = intValue;
					return true;
				}

				if (long.TryParse((string)value, out longValue))
				{
					result = longValue;
					return true;
				}

				if (double.TryParse((string)value, out doubleValue))
				{
					result = doubleValue;
					return true;
				}
			}

			result = null;
			return false;
		}

		/// <summary>
		/// Applies a format specifier to an object.
		/// </summary>
		private static string ApplyFormat(object value, string format)
		{
			if (value is IFormattable)
				return ((IFormattable) value).ToString(format, null);

			var template = string.Format("{{0:{0}}}", format);
			return string.Format(template, value);
		}

		/// <summary>
		/// Attempts to cast or convert an object to a DateTime.
		/// </summary>
		private static bool TryGetDateTime(object value, out DateTime? result)
		{
			if (value is DateTime)
			{
				result = (DateTime)value;
				return true;
			}

			if (value is string)
			{
				DateTime dateTime;
				if (DateTime.TryParse((string)value, out dateTime))
				{
					result = dateTime;
					return true;
				}
			}

			result = null;
			return false;
		}

		/// <summary>
		/// Applies a Word date format to a DateTime value.
		/// </summary>
		private static string ApplyDateTimeFormat(DateTime value, string format)
		{
			// Given format string in Microsoft Word format, convert to an equivelent .NET format string.

			if (ElapsedExpr.IsMatch(format))
				throw new Exception("Elapsed time expressions (i.e.: [h], [m], [ss]) are not supported.");

			// milliseconds are not supported
			if (MillisecondExpr.IsMatch(format))
				throw new Exception("Millisecond expressions (i.e.: .0000) are not supported.");

			// Default to months instead of minutes.  Determine when to
			// use minutes based on proximity to hour and/or second components.
			format = MinuteToMonthExpr.Replace(format, "M");

			// If using hour component and no am/pm is specified, convert to use 0-24 format.
			if (format.Contains("h") && !AmPmExpr.IsMatch(format))
				format = format.Replace("h", "H");

			// Single letter month abbreviation.
			format = FiveCharMonthExpr.Replace(format, m => "MMM" + RandomMarker);

			// Convert to minutes when preceeded by hours
			format = HourMinuteExpr.Replace(format, m =>
			{
				if (MinutesExpr.Match(m.Value).Value.Length > 2)
					throw new Exception("Invalid Format: minute component cannot be more than two characters (" + m.Value + ".");

				return m.Value.Replace("M", "m");
			});

			// Convert to minutes when followed by seconds
			format = MinuteSecondExpr.Replace(format, m =>
			{
				if (MinutesExpr.Match(m.Value).Value.Length > 2)
					throw new Exception("Invalid Format: minute component cannot be more than two characters (" + m.Value + ".");

				return m.Value.Replace("M", "m");
			});

			//// replace ".00..." with ".ff..."
			//format = millisecondExpr.Replace(format, m =>
			//{
			//    string replace = ".";
			//    for (int i = 0; i < m.Value.Length - 1; i++)
			//        replace += "f";
			//    return replace;
			//});

			// Single character formats are not supported, they are interpretted as "special" formats.
			if (format.Length == 1)
				format = "%" + format;
			else
				format = format.Replace("am/pm", RandomMarker + "tt") // convert result to lower-case
					.Replace("a/p", RandomMarker + "t") // convert result to lower-case
					.Replace("AM/PM", "tt")
					.Replace("A/P", "t");

			var str = value.ToString(format);

			// Use markers to convert to lower-case am/pm and a/p.
			str = str.Replace(RandomMarker + "PM", "pm")
				.Replace(RandomMarker + "AM", "am")
				.Replace(RandomMarker + "P", "p")
				.Replace(RandomMarker + "A", "a");

			// Single letter month abbreviation - replace "Jan<marker>" with "J"
			foreach (var abbrev in MonthAbbreviations)
				str = str.Replace(abbrev + RandomMarker, abbrev.Substring(0, 1));

			return str;
		}
	}
}
