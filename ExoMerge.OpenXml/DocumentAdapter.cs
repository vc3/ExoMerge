using System.Collections.Generic;
using System.Linq;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ExoMerge.Documents;
using ExoMerge.OpenXml.Extensions;

namespace ExoMerge.OpenXml
{
	public class DocumentAdapter : IDocumentAdapter<WordprocessingDocument, OpenXmlElement>
	{
		DocumentNodeType IDocumentAdapter<WordprocessingDocument, OpenXmlElement>.GetNodeType(OpenXmlElement node)
		{
			if (node is Run)
				return DocumentNodeType.Run;

			if (node is Paragraph)
				return DocumentNodeType.Paragraph;

			if (node is Table)
				return DocumentNodeType.Table;

			if (node is TableRow)
				return DocumentNodeType.TableRow;

			if (node is TableCell)
				return DocumentNodeType.TableCell;

			return DocumentNodeType.Unknown;
		}

		OpenXmlElement IDocumentAdapter<WordprocessingDocument, OpenXmlElement>.GetParent(OpenXmlElement node)
		{
			return node.Parent;
		}

		OpenXmlElement IDocumentAdapter<WordprocessingDocument, OpenXmlElement>.GetAncestor(OpenXmlElement node, DocumentNodeType type)
		{
			switch (type)
			{
				case DocumentNodeType.Paragraph:
					return node.Ancestors<Paragraph>().FirstOrDefault();

				case DocumentNodeType.Table:
					return node.Ancestors<Table>().FirstOrDefault();

				case DocumentNodeType.TableRow:
					return node.Ancestors<TableRow>().FirstOrDefault();

				case DocumentNodeType.TableCell:
					return node.Ancestors<TableCell>().FirstOrDefault();
			}

			return null;
		}

		OpenXmlElement IDocumentAdapter<WordprocessingDocument, OpenXmlElement>.GetNextSibling(OpenXmlElement node)
		{
			return node.NextSibling();
		}

		OpenXmlElement IDocumentAdapter<WordprocessingDocument, OpenXmlElement>.GetPreviousSibling(OpenXmlElement node)
		{
			return node.PreviousSibling();
		}

		void IDocumentAdapter<WordprocessingDocument, OpenXmlElement>.Remove(OpenXmlElement node)
		{
			node.Remove();
		}

		IEnumerable<OpenXmlElement> IDocumentAdapter<WordprocessingDocument, OpenXmlElement>.GetChildren(WordprocessingDocument document)
		{
			return document.MainDocumentPart.Document.ChildElements;
		}

		bool IDocumentAdapter<WordprocessingDocument, OpenXmlElement>.IsComposite(OpenXmlElement node)
		{
			if (node is Run)
				return false;

			return node is OpenXmlCompositeElement;
		}

		IEnumerable<OpenXmlElement> IDocumentAdapter<WordprocessingDocument, OpenXmlElement>.GetChildren(OpenXmlElement parent)
		{
			return parent.ChildElements;
		}

		OpenXmlElement IDocumentAdapter<WordprocessingDocument, OpenXmlElement>.GetFirstChild(OpenXmlElement parent)
		{
			return parent.FirstChild;
		}

		OpenXmlElement IDocumentAdapter<WordprocessingDocument, OpenXmlElement>.GetLastChild(OpenXmlElement parent)
		{
			return parent.LastChild;
		}

		OpenXmlElement IDocumentAdapter<WordprocessingDocument, OpenXmlElement>.CreateTextRun(WordprocessingDocument document, string text)
		{
			return new Run(new Text(text));
		}

		bool IsTextRun(OpenXmlElement node)
		{
			return node is Run;
		}

		string IDocumentAdapter<WordprocessingDocument, OpenXmlElement>.GetText(OpenXmlElement run)
		{
			return run.ChildElements.Cast<Text>().Single().Text;
		}

		void IDocumentAdapter<WordprocessingDocument, OpenXmlElement>.SetText(OpenXmlElement run, string text)
		{
			run.ChildElements.Cast<Text>().Single().Text = text;
		}

		bool IDocumentAdapter<WordprocessingDocument, OpenXmlElement>.IsNonVisibleMarker(OpenXmlElement node)
		{
			return false;
		}

		void IDocumentAdapter<WordprocessingDocument, OpenXmlElement>.InsertAfter(OpenXmlElement newNode, OpenXmlElement insertAfter)
		{
			insertAfter.Parent.InsertAfter(newNode, insertAfter);
		}

		void IDocumentAdapter<WordprocessingDocument, OpenXmlElement>.InsertBefore(OpenXmlElement newNode, OpenXmlElement insertBefore)
		{
			insertBefore.Parent.InsertBefore(newNode, insertBefore);
		}

		void IDocumentAdapter<WordprocessingDocument, OpenXmlElement>.AppendChild(OpenXmlElement parent, OpenXmlElement newChild)
		{
			parent.AppendChild(newChild);
		}

		OpenXmlElement IDocumentAdapter<WordprocessingDocument, OpenXmlElement>.Clone(OpenXmlElement node, bool recursive)
		{
			return node.Clone(recursive);
		}
	}
}
