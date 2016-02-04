using System.Collections.Generic;
using Aspose.Words;
using Aspose.Words.Fields;
using ExoMerge.Aspose.Extensions;
using ExoMerge.Aspose.MergeFields;

namespace ExoMerge.Aspose.Common
{
	/// <summary>
	/// A class used to represent a field which resides within a single composite (i.e. paragraph) node.
	/// </summary>
	public class InlineField
	{
		internal InlineField(FieldStart start)
		{
			Start = start;
		}

		/// <summary>
		/// Gets the field start node.
		/// </summary>
		public FieldStart Start { get; private set; }

		/// <summary>
		/// Gets the field end node.
		/// </summary>
		public FieldEnd GetEnd()
		{
			return FindMatchingEnd(Start);
		}

		/// <summary>
		/// Determines if the field is a valid inline field.
		/// </summary>
		public bool IsValid()
		{
			return GetEnd() != null;
		}

		/// <summary>
		/// Gets the code of the field, e.g. " MERGEFIELD MyField ".
		/// </summary>
		public string GetCode()
		{
			try
			{
				return Start.ParseInlineField();
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Find the FieldEnd node that matches the field's start node.
		/// </summary>
		private static FieldEnd FindMatchingEnd(FieldStart start)
		{
			try
			{
				FieldEnd end;
				start.ParseInlineField(out end);
				return end;
			}
			catch
			{
				return null;
			}
		}

		/// <summary>
		/// Gets the node that make up the inline field.
		/// </summary>
		public IEnumerable<Node> GetNodes()
		{
			var end = GetEnd();

			if (end == null)
				yield break;

			for (Node node = Start; node != null; node = node.NextSibling)
			{
				yield return node;
				if (node == end)
					break;
			}
		}

		/// <summary>
		/// Create an inline field for the given field start node.
		/// </summary>
		public static InlineField Create(FieldStart start)
		{
			switch (start.FieldType)
			{
				case FieldType.FieldMergeField:
					return new MergeField(start);
				case FieldType.FieldHyperlink:
					return new HyperlinkField(start);
				default:
					return new InlineField(start);
			}
		}

		/// <summary>
		/// Attempt to create an inline field for the given start field.
		/// </summary>
		public static bool TryParse(FieldStart start, out InlineField field)
		{
			var end = FindMatchingEnd(start);

			if (end != null)
			{
				field = Create(start);
				return true;
			}

			field = null;
			return false;
		}
	}
}
