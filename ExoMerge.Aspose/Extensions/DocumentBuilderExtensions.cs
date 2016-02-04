using System;
using Aspose.Words;

namespace ExoMerge.Aspose.Extensions
{
	public static class DocumentBuilderExtensions
	{
		private static void InsertComment(DocumentBuilder builder, Comment comment, string text)
		{
			// http://www.aspose.com/community/forums/permalink/49509/49519/showthread.aspx#49519
			comment.Paragraphs.Add(new Paragraph(builder.Document));
			comment.FirstParagraph.Runs.Add(new Run(builder.Document, text));

			builder.CurrentParagraph.AppendChild(comment);
		}

		/// <summary>
		/// Insert a comment into the document at the current paragraph.
		/// </summary>
		/// <param name="builder">The document builder to use to insert the comment.</param>
		/// <param name="text">The text of the comment.</param>
		/// <returns></returns>
		public static Comment InsertComment(this DocumentBuilder builder, string text)
		{
			var comment = new Comment(builder.Document);
			InsertComment(builder, comment, text);
			return comment;
		}

		/// <summary>
		/// Insert a comment into the document at the current paragraph.
		/// </summary>
		/// <param name="builder">The document builder to use to insert the comment.</param>
		/// <param name="text">The text of the comment.</param>
		/// <param name="author">The author's name.</param>
		/// <param name="initials">The author's initials.</param>
		/// <param name="date">The date of the comment.</param>
		/// <returns></returns>
		public static Comment InsertComment(this DocumentBuilder builder, string text, string author, string initials, DateTime date)
		{
			var comment = new Comment(builder.Document, author, initials, date);
			InsertComment(builder, comment, text);
			return comment;
		}
	}
}
