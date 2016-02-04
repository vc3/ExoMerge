using System;
using Aspose.Words;

namespace ExoMerge.Aspose.Common
{
	/// <summary>
	/// Represents a range of nodes in a document.
	/// </summary>
	public class NodeRange
	{
		/// <summary>
		/// Creates a new node range.
		/// </summary>
		/// <param name="start">The start node.</param>
		/// <param name="end">The end node.</param>
		public NodeRange(Node start, Node end)
		{
			if (start.Document != end.Document)
				throw new InvalidOperationException("Cannot create a node range between nodes in different documents.");

			Document = start.Document;
			Start = start;
			End = end;
		}

		/// <summary>
		/// Gets the document that the range of nodes is contained within.
		/// </summary>
		public DocumentBase Document { get; private set; }

		/// <summary>
		/// Gets the start node of the range.
		/// </summary>
		public Node Start { get; private set; }

		/// <summary>
		/// Gets the end node of the range.
		/// </summary>
		public Node End { get; private set; }
	}
}
