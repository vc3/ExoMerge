using System.Text.RegularExpressions;
using Aspose.Words.Fields;

namespace ExoMerge.Aspose.Common
{
	/// <summary>
	/// Class used to represent a HYPERLINK field.
	/// </summary>
	internal class HyperlinkField : InlineField
	{
		private static readonly Regex FieldCodeParser = new Regex("^\\s*(?<type>[A-Za-z]+)\\s+(?<value>.*[^\\s])\\s*$", RegexOptions.IgnoreCase);

		internal HyperlinkField(FieldStart start)
			: base(start)
		{
		}

		/// <summary>
		/// Gets the value of the hyperlink, e.g. "https://mysite.com".
		/// </summary>
		public string GetValue(out int startIndex, out int endIndex)
		{
			var code = GetCode();

			if (code != null)
			{
				var match = FieldCodeParser.Match(code);

				if (match.Success)
				{
					var value = match.Groups["value"];

					startIndex = value.Index;
					endIndex = value.Index + value.Length - 1;

					if (value.Value.StartsWith("\"") && value.Value.EndsWith("\""))
					{
						startIndex += 1;
						endIndex -= 1;
					}

					return value.Value;
				}
			}

			startIndex = -1;
			endIndex = -1;
			return null;
		}

		/// <summary>
		/// Attempts to parse a field, starting at the given field start node, as a hyperlink field.
		/// </summary>
		public static bool TryParse(FieldStart start, out HyperlinkField field)
		{
			if (start.FieldType == FieldType.FieldHyperlink)
			{
				InlineField inlineField;
				if (InlineField.TryParse(start, out inlineField))
				{
					field = (HyperlinkField)inlineField;
					return true;
				}
			}

			field = null;
			return false;
		}
	}
}
