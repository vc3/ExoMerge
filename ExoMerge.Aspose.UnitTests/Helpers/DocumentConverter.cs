using System;
using System.Collections.Generic;
using System.Globalization;
using Aspose.Words;
using Aspose.Words.Fields;
using ExoMerge.Aspose.Common;
using ExoMerge.Aspose.Extensions;
using ExoMerge.Aspose.UnitTests.Extensions;

namespace ExoMerge.Aspose.UnitTests.Helpers
{
	internal static class DocumentConverter
	{
		internal static string ConvertDisplayToCode(string displayText)
		{
			var leftBraceCode = "[" + Guid.NewGuid().ToString().Substring(0, 8) + "]";
			var separatorCode = "[" + Guid.NewGuid().ToString().Substring(0, 8) + "]";
			var rightBraceCode = "[" + Guid.NewGuid().ToString().Substring(0, 8) + "]";

			return displayText
				.Replace("\\{", leftBraceCode)
				.Replace("{", FieldCharacters.Start.ToString(CultureInfo.InvariantCulture))
				.Replace(leftBraceCode, "}")
				.Replace("\\|", separatorCode)
				.Replace("|", FieldCharacters.Separator.ToString(CultureInfo.InvariantCulture))
				.Replace(separatorCode, "|")
				.Replace("\\}", rightBraceCode)
				.Replace("}", FieldCharacters.End.ToString(CultureInfo.InvariantCulture))
				.Replace(rightBraceCode, "}");
		}

		internal static string ConvertCodeToDisplay(string displayText)
		{
			var leftBraceCode = "[" + Guid.NewGuid().ToString().Substring(0, 8) + "]";
			var separatorCode = "[" + Guid.NewGuid().ToString().Substring(0, 8) + "]";
			var rightBraceCode = "[" + Guid.NewGuid().ToString().Substring(0, 8) + "]";

			return displayText
				.Replace("{", leftBraceCode)
				.Replace(FieldCharacters.Start.ToString(CultureInfo.InvariantCulture), "{")
				.Replace(leftBraceCode, "\\{")
				.Replace("|", separatorCode)
				.Replace(FieldCharacters.Separator.ToString(CultureInfo.InvariantCulture), "|")
				.Replace(separatorCode, "\\|")
				.Replace("}", rightBraceCode)
				.Replace(FieldCharacters.End.ToString(CultureInfo.InvariantCulture), "}")
				.Replace(rightBraceCode, "\\}");
		}

		public static Document FromDisplayCode(string source)
		{
			Field[] fields;
			return FromDisplayCode(source, out fields);
		}

		public static Document FromDisplayCode(string source, out Field[] fields)
		{
			var builder = new DocumentBuilder();
			fields = builder.InsertNodesFromText(ConvertDisplayToCode(source));
			return builder.Document;
		}

		public static Document FromStrings(string[] strings)
		{
			Run[] runs;
			return FromStrings(strings, out runs);
		}

		public static Document FromStrings(string[] strings, out Run[] runs)
		{
			var builder = new DocumentBuilder();

			var runList = new List<Run>();

			var comments = new Dictionary<string, Comment>();

			foreach (var str in strings)
			{
				// Bookmark
				if (str.StartsWith("@{") && str.EndsWith("}"))
				{
					if (str.StartsWith("@{/"))
					{
						var bookmarkName = str.Substring(3, str.Length - 4);
						builder.EndBookmark(bookmarkName);
					}
					else
					{
						var bookmarkName = str.Substring(2, str.Length - 3);
						builder.StartBookmark(bookmarkName);
					}

					continue;
				}

				// Comment
				if (str.StartsWith("!{") && str.EndsWith("}"))
				{
					if (str.StartsWith("!{/"))
					{
						var commentName = str.Substring(3, str.Length - 4);

						var comment = comments[commentName];

						var commentEnd = new CommentRangeEnd(builder.Document, comment.Id);

						builder.InsertNode(commentEnd);
					}
					else
					{
						var commentName = str.Substring(2, str.Length - 3);

						var comment = new Comment(builder.Document, "", "", DateTime.Now);

						comments.Add(commentName, comment);

						comment.SetText("Comment: " + commentName);

						builder.InsertNode(comment);

						var commentStart = new CommentRangeStart(builder.Document, comment.Id);

						builder.InsertNode(commentStart);
					}

					continue;
				}

				runList.Add(builder.InsertRun(str));
			}

			runs = runList.ToArray();

			return builder.Document;
		}
	}
}
