using System.Collections.Generic;

namespace ExoMerge.Documents
{
	/// <summary>
	/// A class that is responsible for manipulating a document of type <see cref="TDocument"/>.
	/// </summary>
	/// <typeparam name="TDocument">The type of document to manipulate.</typeparam>
	/// <typeparam name="TNode">The type that is used to represent nodes in the document.</typeparam>
	public interface IDocumentAdapter<in TDocument, TNode>
	{
		/// <summary>
		/// Gets the type of the given node.
		/// </summary>
		DocumentNodeType GetNodeType(TNode node);

		/// <summary>
		/// Get the node's parent.
		/// </summary>
		TNode GetParent(TNode node);

		/// <summary>
		/// Get the node's closest ancestor of the given type.
		/// </summary>
		TNode GetAncestor(TNode node, DocumentNodeType type);

		/// <summary>
		/// Get the node's next sibling.
		/// </summary>
		TNode GetNextSibling(TNode node);

		/// <summary>
		/// Get the node's previous sibling.
		/// </summary>
		TNode GetPreviousSibling(TNode node);

		/// <summary>
		/// Remove the node from the document.
		/// </summary>
		void Remove(TNode node);

		/// <summary>
		/// Get the document's immediate children.
		/// </summary>
		IEnumerable<TNode> GetChildren(TDocument document);

		/// <summary>
		/// Determine whether the given node is a composite node.
		/// </summary>
		bool IsComposite(TNode node);

		/// <summary>
		/// Get the node's immediate children.
		/// </summary>
		IEnumerable<TNode> GetChildren(TNode node);

		/// <summary>
		/// Get the node's first child.
		/// </summary>
		TNode GetFirstChild(TNode node);

		/// <summary>
		/// Get the node's last child.
		/// </summary>
		TNode GetLastChild(TNode node);

		/// <summary>
		/// Create a run of text.
		/// </summary>
		TNode CreateTextRun(TDocument document, string text);

		/// <summary>
		/// Get the node's text.
		/// </summary>
		string GetText(TNode node);

		/// <summary>
		/// Set the node's text.
		/// </summary>
		void SetText(TNode node, string text);

		/// <summary>
		/// Determines whether the given node is a non-visible marker element.
		/// </summary>
		bool IsNonVisibleMarker(TNode node);

		/// <summary>
		/// Insert the node after the given node.
		/// </summary>
		void InsertAfter(TNode newNode, TNode insertAfter);

		/// <summary>
		/// Insert the node before the given node.
		/// </summary>
		void InsertBefore(TNode newNode, TNode insertBefore);

		/// <summary>
		/// Append a new child to the given node.
		/// </summary>
		void AppendChild(TNode parent, TNode newChild);

		/// <summary>
		/// Create a clone of the node.
		/// </summary>
		TNode Clone(TNode node, bool recursive);
	}
}
