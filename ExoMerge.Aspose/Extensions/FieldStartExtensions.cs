using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Aspose.Words;
using Aspose.Words.Fields;

namespace ExoMerge.Aspose.Extensions
{
	public static class FieldStartExtensions
	{
		private static readonly string FieldStart = ((char) 19).ToString(CultureInfo.InvariantCulture);
		private static readonly string FieldSeparator = ((char)20).ToString(CultureInfo.InvariantCulture);
		private static readonly string FieldEnd = ((char)21).ToString(CultureInfo.InvariantCulture);

		private static string GetText(FieldStart start, string code, FieldSeparator separator = null, string result = null, FieldEnd end = null, bool useRawFieldChars = false)
		{
			return string.Format("{0}{1}{2}{3}{4}",
				start != null ? useRawFieldChars ? FieldStart : "{" : "",
				code ?? "",
				separator != null ? useRawFieldChars ? FieldSeparator : "|" : "",
				result ?? "",
				end != null ? useRawFieldChars ? FieldEnd : "}" : "");
		}

		private static string ParseInlineCode(FieldStart start, out Node lastNode)
		{
			var codeBuilder = new StringBuilder();

			lastNode = start;

			foreach (var sibling in start.GetFollowingSiblings().TakeWhile(n => !(n is FieldSeparator)))
			{
				var run = sibling as Run;
				if (run != null)
					codeBuilder.Append(run.GetText());
				else
					throw new Exception(string.Format("Found unexpected node of type '{0}' after \"{1}\".", sibling.GetType().Name, GetText(start, codeBuilder.ToString())));

				lastNode = sibling;
			}

			return codeBuilder.ToString();
		}

		private static string ParseInlineResult(FieldStart start, string code, FieldSeparator separator, out Node lastNode)
		{
			var resultBuilder = new StringBuilder();

			lastNode = separator;

			foreach (var sibling in separator.GetFollowingSiblings().TakeWhile(n => !(n is FieldEnd)))
			{
				var run = sibling as Run;
				if (run != null)
					resultBuilder.Append(run.GetText());
				else if (!(sibling is BookmarkStart) && !(sibling is BookmarkEnd))
					throw new Exception(string.Format("Found unexpected node of type '{0}' after \"{1}\".", sibling.GetType().Name, GetText(start, code, separator, resultBuilder.ToString())));

				lastNode = sibling;
			}

			return resultBuilder.ToString();
		}

		/// <summary>
		/// Gets the code for a field, starting at the given field start node.
		/// If the field is not well formed, then an exception is thrown.
		/// </summary>
		public static string ParseInlineField(this FieldStart start)
		{
			FieldSeparator separator;
			string result;
			FieldEnd end;

			return start.ParseInlineField(out separator, out result, out end);
		}

		/// <summary>
		/// Gets the code for a field, starting at the given field start node.
		/// If the field is not well formed, then an exception is thrown.
		/// </summary>
		public static string ParseInlineField(this FieldStart start, out FieldEnd end)
		{
			FieldSeparator separator;
			string result;

			return start.ParseInlineField(out separator, out result, out end);
		}

		/// <summary>
		/// Gets the code for a field, starting at the given field start node.
		/// If the field is not well formed, then an exception is thrown.
		/// </summary>
		public static string ParseInlineField(this FieldStart start, out FieldSeparator separator, out string result, out FieldEnd end)
		{
			if (start.NextSibling == null)
				throw new Exception("Found field start with no next sibling.");

			Node lastNode;

			var code = ParseInlineCode(start, out lastNode);

			if (lastNode == null || lastNode == start)
				throw new Exception(string.Format("Didn't find field code in \"{0}\".", GetText(start, code, separator: start.NextSibling as FieldSeparator)));

			separator = lastNode.NextSibling as FieldSeparator;

			if (separator == null)
				throw new Exception(string.Format("Did not find node of type 'FieldSeparator' after \"{0}\".", GetText(start, code)));

			result = ParseInlineResult(start, code, separator, out lastNode);

			end = separator.GetFollowingSiblings().OfType<FieldEnd>().FirstOrDefault();

			if (end == null)
				throw new Exception(string.Format("Did not find node of type 'FieldEnd' after \"{0}\".", GetText(start, code, separator, result)));

			return code;
		}
	}
}
