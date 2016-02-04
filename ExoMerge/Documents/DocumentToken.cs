using ExoMerge.Analysis;
using JetBrains.Annotations;

namespace ExoMerge.Documents
{
	/// <summary>
	/// Represents a token in a document.
	/// </summary>
	/// <typeparam name="TNode">The type of node that marks the start and end of a token.</typeparam>
	public class DocumentToken<TNode> : IToken<TNode, TNode>
		where TNode : class
	{
		/// <summary>
		/// Creates a new document token.
		/// </summary>
		/// <param name="startNode">The start node.</param>
		/// <param name="endNode">The end node.</param>
		/// <param name="value">The textual value.</param>
		/// <param name="encoding">The text encoding that applies to the token.</param>
		public DocumentToken([NotNull] TNode startNode, [NotNull] TNode endNode, [NotNull] string value, DocumentTextEncoding encoding = DocumentTextEncoding.None)
		{
			Value = value;
			Start = startNode;
			End = endNode;
			Encoding = encoding;
		}

		/// <summary>
		/// The textual value of the token.
		/// </summary>
		public string Value { get; private set; }

		/// <summary>
		/// The node that marks the start of the token.
		/// </summary>
		public TNode Start { get; private set; }

		/// <summary>
		/// The node that marks the end of the token.
		/// </summary>
		public TNode End { get; private set; }

		/// <summary>
		/// The encoding of the token's text.
		/// </summary>
		public DocumentTextEncoding Encoding { get; private set; }
	}
}
