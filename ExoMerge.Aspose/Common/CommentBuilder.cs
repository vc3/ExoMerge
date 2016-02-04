using System;
using Aspose.Words;

namespace ExoMerge.Aspose.Common
{
	public class CommentBuilder
	{
		/// <summary>
		/// Sets a comments text.
		/// </summary>
		/// <param name="comment">The commit to modify.</param>
		/// <param name="text">The new comment text.</param>
		private static void SetCommentText(Comment comment, string text)
		{
			if (comment.ChildNodes.Count > 0)
				comment.ChildNodes.Clear();

			// Insert some text into the comment.
			var commentParagraph = new Paragraph(comment.Document);
			commentParagraph.AppendChild(new Run(comment.Document, text));
			comment.AppendChild(commentParagraph);
		}

		/// <summary>
		/// Inserts a comment into a document.
		/// </summary>
		/// <param name="nodeRange">The range of nodes to insert the comment around.</param>
		/// <param name="comment">The comment to insert.</param>
		private static void InsertComment(NodeRange nodeRange, Comment comment)
		{
			// Create CommentRangeStart and CommentRangeEnd.
			var commentStart = new CommentRangeStart(nodeRange.Document, comment.Id);
			var commentEnd = new CommentRangeEnd(nodeRange.Document, comment.Id);

			// Insert the comment and range nodes at the appropriate locations.
			nodeRange.Start.ParentNode.InsertBefore(comment, nodeRange.Start);
			nodeRange.Start.ParentNode.InsertBefore(commentStart, nodeRange.Start);
			nodeRange.End.ParentNode.InsertAfter(commentEnd, nodeRange.End);
		}

		/// <summary>
		/// Wraps a range of nodes in a comment.
		/// </summary>
		/// <param name="nodeRange">The range of nodes to wrap in a comment.</param>
		/// <param name="text">The text of the comment.</param>
		/// <returns>The new comment.</returns>
		public static Comment WrapInComment(NodeRange nodeRange, string text)
		{
			// http://www.aspose.com/community/forums/permalink/49509/224604/showthread.aspx#224604
			var comment = new Comment(nodeRange.Document);
			SetCommentText(comment, text);
			InsertComment(nodeRange, comment);
			return comment;
		}

		/// <summary>
		/// Wraps a range of nodes in a comment.
		/// </summary>
		/// <param name="nodeRange">The range of nodes to wrap in a comment.</param>
		/// <param name="text">The text of the comment.</param>
		/// <param name="author">The author's name.</param>
		/// <param name="initials">The author's initials.</param>
		/// <param name="date">The date of the comment.</param>
		/// <returns>The new comment.</returns>
		public static Comment WrapInComment(NodeRange nodeRange, string text, string author, string initials, DateTime date)
		{
			// http://www.aspose.com/community/forums/permalink/49509/224604/showthread.aspx#224604
			var comment = new Comment(nodeRange.Document, author, initials, date);
			SetCommentText(comment, text);
			InsertComment(nodeRange, comment);
			return comment;
		}
	}
}
