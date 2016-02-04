using System.Text.RegularExpressions;
using Aspose.Words.Fields;
using ExoMerge.Aspose.Common;

namespace ExoMerge.Aspose.MergeFields
{
	/// <summary>
	/// A class used to attempt to parse field nodes as a MERGEFIELD.
	/// </summary>
	public class MergeField : InlineField
	{
		private static readonly Regex Parser = new Regex("^\\s*(?<type>MERGEFIELD)\\s+(?<name>.*[^\\s])\\s*$", RegexOptions.IgnoreCase);

		internal MergeField(FieldStart start)
			: base(start)
		{
		}

		/// <summary>
		/// Gets the merge field name, e.g. "MyField".
		/// </summary>
		public string Name
		{
			get
			{
				var code = GetCode();

				if (code != null && Parser.IsMatch(code))
					return Parser.Replace(code, "${name}");

				return null;
			}
		}

		/// <summary>
		/// Attempts to parse a field, starting at the given field start node, as a merge field.
		/// If the field is not well formed, then an exception is thrown. If the field is not a
		/// merge field, then the return value will be false, indicating that the input data was
		/// valid but could not be parsed as a merge field.
		/// </summary>
		public static bool TryParse(FieldStart start, out MergeField field)
		{
			if (start.FieldType == FieldType.FieldMergeField)
			{
				InlineField inlineField;
				if (InlineField.TryParse(start, out inlineField))
				{
					field = (MergeField)inlineField;
					return true;
				}
			}

			field = null;
			return false;
		}
	}
}
