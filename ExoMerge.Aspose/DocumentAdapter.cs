using System;
using System.Collections.Generic;
using System.Linq;
using Aspose.Words;
using ExoMerge.Documents;

namespace ExoMerge.Aspose
{
	public class DocumentAdapter : IDocumentAdapter<Document, Node>
	{
		/// <summary>
		/// Gets the type of the given node.
		/// </summary>
		DocumentNodeType IDocumentAdapter<Document, Node>.GetNodeType(Node node)
		{
			switch (node.NodeType)
			{
				case NodeType.Table:
					return DocumentNodeType.Table;

				case NodeType.Row:
					return DocumentNodeType.TableRow;

				case NodeType.Cell:
					return DocumentNodeType.TableCell;

				case NodeType.Paragraph:
					return DocumentNodeType.Paragraph;

				case NodeType.Run:
					return DocumentNodeType.Run;
			}

			return DocumentNodeType.Unknown;
		}

		/// <summary>
		/// Get the node's parent.
		/// </summary>
		Node IDocumentAdapter<Document, Node>.GetParent(Node node)
		{
			return node.ParentNode;
		}

		/// <summary>
		/// Get the node's closest ancestor of the given type.
		/// </summary>
		Node IDocumentAdapter<Document, Node>.GetAncestor(Node node, DocumentNodeType type)
		{
			switch (type)
			{
				case DocumentNodeType.Table:
					return node.GetAncestor(NodeType.Table);

				case DocumentNodeType.TableRow:
					return node.GetAncestor(NodeType.Row);

				case DocumentNodeType.TableCell:
					return node.GetAncestor(NodeType.Cell);

				case DocumentNodeType.Paragraph:
					return node.GetAncestor(NodeType.Paragraph);
			}

			return null;
		}

		/// <summary>
		/// Get the node's next sibling.
		/// </summary>
		Node IDocumentAdapter<Document, Node>.GetNextSibling(Node node)
		{
			return node.NextSibling;
		}

		/// <summary>
		/// Get the node's previous sibling.
		/// </summary>
		Node IDocumentAdapter<Document, Node>.GetPreviousSibling(Node node)
		{
			return node.PreviousSibling;
		}

		/// <summary>
		/// Remove the node from the document.
		/// </summary>
		void IDocumentAdapter<Document, Node>.Remove(Node node)
		{
			node.Remove();
		}

		/// <summary>
		/// Get the document's immediate children.
		/// </summary>
		IEnumerable<Node> IDocumentAdapter<Document, Node>.GetChildren(Document document)
		{
			return document.ChildNodes.OfType<Node>();
		}

		/// <summary>
		/// Determine whether the given node is a composite node.
		/// </summary>
		bool IDocumentAdapter<Document, Node>.IsComposite(Node node)
		{
			return node.IsComposite;
		}

		/// <summary>
		/// Get the node's immediate children.
		/// </summary>
		IEnumerable<Node> IDocumentAdapter<Document, Node>.GetChildren(Node node)
		{
			var composite = node as CompositeNode;
			if (composite != null)
				return composite.ChildNodes.OfType<Node>();

			return null;
		}

		/// <summary>
		/// Get the node's first child.
		/// </summary>
		Node IDocumentAdapter<Document, Node>.GetFirstChild(Node node)
		{
			var composite = node as CompositeNode;
			if (composite != null)
				return composite.FirstChild;

			return null;
		}

		/// <summary>
		/// Get the node's last child.
		/// </summary>
		Node IDocumentAdapter<Document, Node>.GetLastChild(Node node)
		{
			var composite = node as CompositeNode;
			if (composite != null)
				return composite.LastChild;

			return null;
		}

		/// <summary>
		/// Create a text run node.
		/// </summary>
		Node IDocumentAdapter<Document, Node>.CreateTextRun(Document document, string text)
		{
			return new Run(document, text);
		}

		/// <summary>
		/// Get the run's text.
		/// </summary>
		string IDocumentAdapter<Document, Node>.GetText(Node node)
		{
			var run = node as Run;
			if (run != null)
				return run.Text;

			return null;
		}

		/// <summary>
		/// Set the node's text.
		/// </summary>
		void IDocumentAdapter<Document, Node>.SetText(Node node, string text)
		{
			var run = node as Run;
			if (run == null)
				throw new InvalidOperationException("Cannot set the text of nodes of type '" + node.NodeType + "'.");
			run.Text = text;
		}

		/// <summary>
		/// Determines whether the given node is a non-visible marker element.
		/// </summary>
		bool IDocumentAdapter<Document, Node>.IsNonVisibleMarker(Node node)
		{
			return node is Comment || node is CommentRangeStart || node is CommentRangeEnd
				// Editable range was added on 2015.10.31
				// http://www.aspose.com/api/net/words/T_Aspose_Words_EditableRangeStart
				//|| node is EditableRangeStart || node is EditableRangeEnd
			       || node is BookmarkStart || node is BookmarkEnd;
		}

		/// <summary>
		/// Insert the node after the given node.
		/// </summary>
		void IDocumentAdapter<Document, Node>.InsertAfter(Node newNode, Node insertAfter)
		{
			try
			{
				insertAfter.ParentNode.InsertAfter(newNode, insertAfter);
			}
			catch (Exception e)
			{
				if (e.Message == "Cannot insert a node of this type at this location.")
					throw new ArgumentException("Cannot insert node of type '" + newNode.NodeType + "' into node of type '" + insertAfter.ParentNode.NodeType + "'.");

				throw;
			}
		}

		/// <summary>
		/// Insert the node before the given node.
		/// </summary>
		void IDocumentAdapter<Document, Node>.InsertBefore(Node newNode, Node insertBefore)
		{
			try
			{
				insertBefore.ParentNode.InsertBefore(newNode, insertBefore);
			}
			catch (Exception e)
			{
				if (e.Message == "Cannot insert a node of this type at this location.")
					throw new ArgumentException("Cannot insert node of type '" + newNode.NodeType + "' into node of type '" + insertBefore.ParentNode.NodeType + "'.");

				throw;
			}
		}

		/// <summary>
		/// Append a new child to the given node.
		/// </summary>
		void IDocumentAdapter<Document, Node>.AppendChild(Node parent, Node newChild)
		{
			var composite = parent as CompositeNode;
			if (composite == null)
				throw new InvalidOperationException("Cannot append children to nodes of type '" + parent.NodeType + "'.");

			try
			{
				composite.AppendChild(newChild);
			}
			catch (Exception e)
			{
				if (e.Message == "Cannot insert a node of this type at this location.")
					throw new ArgumentException("Cannot insert node of type '" + newChild.NodeType + "' into node of type '" + parent.NodeType + "'.");

				throw;
			}
		}

		/// <summary>
		/// Create a clone of the node.
		/// </summary>
		Node IDocumentAdapter<Document, Node>.Clone(Node node, bool recursive)
		{
			return node.Clone(recursive);
		}
	}
}
