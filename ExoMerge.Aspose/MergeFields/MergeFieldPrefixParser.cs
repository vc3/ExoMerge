using System;
using System.Collections.Generic;
using ExoMerge.Analysis;

namespace ExoMerge.Aspose.MergeFields
{
	/// <summary>
	/// Parses tokens as repeatable and conditional blocks by looking for the presence of a keyword
	/// prefix following by a colon ':' and then an expression, e.g. `IfStart:MyProperty`.
	/// </summary>
	/// <typeparam name="TType">The type of the source type, e.g. 'Type'.</typeparam>
	public class MergeFieldPrefixParser<TType> : ITokenParser<TType>
	{
		public MergeFieldPrefixParser()
			: this("Table", null, RegionNameFormat.StartAndEndSuffix)
		{
		}

		public MergeFieldPrefixParser(string repeatableName, string conditionalName)
			: this(repeatableName, conditionalName, RegionNameFormat.StartAndEndSuffix)
		{
		}

		public MergeFieldPrefixParser(string repeatableName, string conditionalName, RegionNameFormat format)
		{
			SwitchParser = new MergeFieldSwitchParser();

			RepeatableBeginPrefix = repeatableName + (format == RegionNameFormat.StartAndEndSuffix ? "Start" : "");
			RepeatableEndPrefix = (format == RegionNameFormat.EndPrefixOnly ? "End" : "") + repeatableName + (format == RegionNameFormat.StartAndEndSuffix ? "End" : "");

			if (conditionalName != null)
			{
				ConditionalBeginPrefix = conditionalName + (format == RegionNameFormat.StartAndEndSuffix ? "Start" : "");
				ConditionalEndPrefix = (format == RegionNameFormat.EndPrefixOnly ? "End" : "") + conditionalName + (format == RegionNameFormat.StartAndEndSuffix ? "End" : "");
			}
		}

		/// <summary>
		/// Gets the object used to parse merge field switches.
		/// </summary>
		public MergeFieldSwitchParser SwitchParser { get; private set; }

		/// <summary>
		/// Determines the format of region start and end names.
		/// </summary>
		public enum RegionNameFormat
		{
			/// <summary>
			/// Both the region start and end markers use a suffix, 'Start' and 'End' respectively.
			/// </summary>
			StartAndEndSuffix,

			/// <summary>
			/// Only the region end marker uses a prefix of 'End'.
			/// </summary>
			EndPrefixOnly
		}

		public string RepeatableBeginPrefix { get; private set; }

		public string RepeatableEndPrefix { get; private set; }

		public string ConditionalBeginPrefix { get; private set; }

		public string ConditionalEndPrefix { get; private set; }

		/// <summary>
		/// Attempt to parse the given token text.
		/// </summary>
		/// <param name="sourceType">The source type.</param>
		/// <param name="tokenValue">The token value to parse.</param>
		/// <returns>The parse result.</returns>
		TokenParseResult ITokenParser<TType>.Parse(TType sourceType, string tokenValue)
		{
			// Split the value to get the prefix and expression.
			// Example: ["List", "FieldName \switchName SwitchValue..."]
			var components = tokenValue.Trim().Split(":".ToCharArray());

			if (components.Length == 0)
				throw new Exception("No token text to parse.");

			string prefix;
			string remainder;

			if (components.Length == 1)
			{
				// If there is only one item in the array, then it's not a list or if field, but still may have switches.
				prefix = null;
				remainder = components[0];
			}
			else
			{
				prefix = components[0];
				remainder = components[1];
			}

			TokenType type;

			if (prefix == null)
				type = TokenType.ContentField;
			else if (prefix == RepeatableBeginPrefix)
				type = TokenType.RepeatableBegin;
			else if (prefix == RepeatableEndPrefix)
				type = TokenType.RepeatableEnd;
			else if (ConditionalBeginPrefix != null && prefix == ConditionalBeginPrefix)
				type = TokenType.ConditionalBeginWithTest;
			else if (ConditionalEndPrefix != null && prefix == ConditionalEndPrefix)
				type = TokenType.ConditionalEnd;
			else
				throw new Exception(string.Format("Unexpected prefix \"{0}\".", prefix));

			KeyValuePair<string, string>[] switches;

			var name = SwitchParser.ExtractOptions(remainder, out switches);

			return new TokenParseResult(type, name != null ? name.Trim() : null, switches);
		}
	}
}
