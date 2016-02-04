using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExoMerge.Documents;
using HtmlAgilityPack;

namespace ExoMerge.UnitTests.Html
{
	class HtmlDocumentAdapter : IDocumentAdapter<HtmlDocument, HtmlNode>
	{
		DocumentNodeType IDocumentAdapter<HtmlDocument, HtmlNode>.GetNodeType(HtmlNode node)
		{
			switch (node.Name)
			{
				case "p":
					return DocumentNodeType.Paragraph;

				case "span":
					return DocumentNodeType.Run;

				case "table":
					return DocumentNodeType.Table;

				case "tr":
					return DocumentNodeType.TableRow;

				case "td":
				case "th":
					return DocumentNodeType.TableCell;
			}

			return DocumentNodeType.Unknown;
		}

		HtmlNode IDocumentAdapter<HtmlDocument, HtmlNode>.GetParent(HtmlNode node)
		{
			return node.ParentNode;
		}

		HtmlNode IDocumentAdapter<HtmlDocument, HtmlNode>.GetAncestor(HtmlNode node, DocumentNodeType type)
		{
			switch (type)
			{
				case DocumentNodeType.Paragraph:
					return node.Ancestors("p").FirstOrDefault();

				case DocumentNodeType.Table:
					return node.Ancestors("table").FirstOrDefault();

				case DocumentNodeType.TableRow:
					return node.Ancestors("tr").FirstOrDefault();

				case DocumentNodeType.TableCell:
					return node.Ancestors().FirstOrDefault(n => n.Name == "td" || n.Name == "th");
			}

			return null;
		}

		HtmlNode IDocumentAdapter<HtmlDocument, HtmlNode>.GetNextSibling(HtmlNode node)
		{
			return node.NextSibling;
		}

		HtmlNode IDocumentAdapter<HtmlDocument, HtmlNode>.GetPreviousSibling(HtmlNode node)
		{
			return node.PreviousSibling;
		}

		void IDocumentAdapter<HtmlDocument, HtmlNode>.Remove(HtmlNode node)
		{
			node.Remove();
		}

		IEnumerable<HtmlNode> IDocumentAdapter<HtmlDocument, HtmlNode>.GetChildren(HtmlDocument document)
		{
			return new HtmlNodeChildrenList(document.DocumentNode);
		}

		bool IDocumentAdapter<HtmlDocument, HtmlNode>.IsComposite(HtmlNode node)
		{
			switch (node.Name)
			{
				case "p":
				case "table":
				case "tbody":
				case "thead":
				case "tfoot":
				case "tr":
				case "th":
				case "td":
					return true;
				default:
					return false;
			}
		}

		IEnumerable<HtmlNode> IDocumentAdapter<HtmlDocument, HtmlNode>.GetChildren(HtmlNode node)
		{
			return new HtmlNodeChildrenList(node);
		}

		HtmlNode IDocumentAdapter<HtmlDocument, HtmlNode>.GetFirstChild(HtmlNode node)
		{
			return node.FirstChild;
		}

		HtmlNode IDocumentAdapter<HtmlDocument, HtmlNode>.GetLastChild(HtmlNode node)
		{
			return node.LastChild;
		}

		HtmlNode IDocumentAdapter<HtmlDocument, HtmlNode>.CreateTextRun(HtmlDocument document, string text)
		{
			return new HtmlNode(HtmlNodeType.Element, document, 0)
			{
				Name = "span", InnerHtml = text,
			};
		}

		string IDocumentAdapter<HtmlDocument, HtmlNode>.GetText(HtmlNode node)
		{
			//if (node.ChildNodes.Count > 0)
			//	return null;

			var text = new StringBuilder();

			foreach (var child in node.ChildNodes)
			{
				var textNode = child as HtmlTextNode;
				if (textNode == null)
					throw new InvalidOperationException("Node of type '" + node.Name + "' has non-text node of type '" + child.Name + "'.");

				text.Append(textNode.Text);
			}

			return text.ToString();
		}

		void IDocumentAdapter<HtmlDocument, HtmlNode>.SetText(HtmlNode node, string text)
		{
			node.ChildNodes.Clear();
			node.ChildNodes.Append(node.OwnerDocument.CreateTextNode(text));
		}

		bool IDocumentAdapter<HtmlDocument, HtmlNode>.IsNonVisibleMarker(HtmlNode node)
		{
			//var textNode = node as HtmlTextNode;
			//if (textNode != null)
			//	return string.IsNullOrWhiteSpace(textNode.Text);

			return false;
		}

		void IDocumentAdapter<HtmlDocument, HtmlNode>.InsertAfter(HtmlNode newNode, HtmlNode insertAfter)
		{
			try
			{
				insertAfter.ParentNode.InsertAfter(newNode, insertAfter);
			}
			catch (Exception e)
			{
				throw new ArgumentException("Cannot insert node of type '" + newNode.NodeType + "' into node of type '" + insertAfter.ParentNode.Name + "'.", e);
			}
		}

		void IDocumentAdapter<HtmlDocument, HtmlNode>.InsertBefore(HtmlNode newNode, HtmlNode insertBefore)
		{
			try
			{
				insertBefore.ParentNode.InsertBefore(newNode, insertBefore);
			}
			catch (Exception e)
			{
				throw new ArgumentException("Cannot insert node of type '" + newNode.NodeType + "' into node of type '" + insertBefore.ParentNode.Name + "'.", e);
			}
		}

		void IDocumentAdapter<HtmlDocument, HtmlNode>.AppendChild(HtmlNode parent, HtmlNode newChild)
		{
			try
			{
				parent.AppendChild(newChild);
			}
			catch (Exception e)
			{
				throw new ArgumentException("Cannot insert node of type '" + newChild.NodeType + "' into node of type '" + parent.Name + "'.", e);
			}
		}

		HtmlNode IDocumentAdapter<HtmlDocument, HtmlNode>.Clone(HtmlNode node, bool recursive)
		{
			return node.CloneNode(recursive || ((IDocumentAdapter<HtmlDocument, HtmlNode>)this).GetNodeType(node) == DocumentNodeType.Run);
		}

		private class HtmlNodeChildrenEnumerator : IEnumerator<HtmlNode>
		{
			private readonly HtmlNode _node;

			public HtmlNodeChildrenEnumerator(HtmlNode node)
			{
				_node = node;
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				if (Current == null)
				{
					Current = _node.FirstChild;
					return Current != null;
				}

				Current = Current.NextSibling;
				return Current != null;
			}

			public void Reset()
			{
				Current = null;
			}

			public HtmlNode Current { get; private set; }

			object IEnumerator.Current
			{
				get { return Current; }
			}
		}

		private class HtmlNodeChildrenList : IEnumerable<HtmlNode>
		{
			private readonly HtmlNode _node;

			public HtmlNodeChildrenList(HtmlNode node)
			{
				_node = node;
			}

			public IEnumerator<HtmlNode> GetEnumerator()
			{
				return new HtmlNodeChildrenEnumerator(_node);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}
		}
	}
}
