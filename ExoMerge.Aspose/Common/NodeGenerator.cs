using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Aspose.Words;
using ExoMerge.Aspose.Extensions;

namespace ExoMerge.Aspose.Common
{
	public static class NodeGenerator
	{
		private const string HtmlLineBreakExpression = @"<p>\s*<br\s*/>\s*</p>";

		/// <summary>
		/// Returns a disconnected array of nodes for the given HTML text.
		/// </summary>
		/// <param name="document">The document to use to create the field nodes.</param>
		/// <param name="html">The HTML text.</param>
		/// <returns>A disconnected array of nodes for the given HTML text.</returns>
		public static Node[] CreateNodesForHtml(Document document, string html)
		{
			var builder = new DocumentBuilder(document);

			var body = (Body)document.SelectSingleNode("//Body");

			var targetParagraph = (CompositeNode)body.AppendChild(new Paragraph(builder.Document));

			builder.MoveTo(targetParagraph);

			var priorNode = (CompositeNode)targetParagraph.PreviousSibling;

			builder.InsertHtml(Regex.Replace(html, HtmlLineBreakExpression, "<p>&nbsp;</p>"));

			// DocumentBuilder seems to insert a paragraph into the base document for positioning/modification of the document.
			// Because we move to a specific spot in the document (and it is generally just before this new paragraph)
			// this extra paragraph is left just hanging around. Once we have inserted our html string, the DocumentBuilder
			// will be positioned on this new paragraph. Since all it is is a carriage return character, we can remove it.
			// This was causing autotests in MailMergeModel to fail, and likely adding extra empty lines to the rendered
			// documents.
			if (builder.CurrentParagraph.Range.Text == "\r" || builder.CurrentParagraph.Range.Text == "\f")
				builder.CurrentParagraph.Remove();

			var nodes = new List<Node>();

			if (targetParagraph.ChildNodes.Count == 0)
			{
				for (Node node = priorNode; node != null && node != targetParagraph; node = node.NextSibling)
				{
					nodes.Add(node);
					node.Remove();
				}

				targetParagraph.Remove();
			}
			else if (priorNode.NextSibling == targetParagraph)
			{
				nodes.AddRange(targetParagraph.ChildNodes.Cast<Node>());
				targetParagraph.Remove();
			}
			else
			{
				for (Node node = priorNode; node != null; node = node.NextSibling)
				{
					nodes.Add(node);
					node.Remove();
				}
			}

			return nodes.ToArray();
		}

		/// <summary>
		/// Returns a disconnected array of nodes that represent a field with the given code.
		/// </summary>
		/// <param name="document">The document to use to create the field nodes.</param>
		/// <param name="code">The field code.</param>
		/// <returns>A disconnected array of nodes that represent a field with the given code.</returns>
		public static Node[] CreateNodesForField(Document document, string code)
		{
			var builder = new DocumentBuilder(document);

			var nodes = new List<Node>();

			var body = (Body)document.SelectSingleNode("//Body");

			var targetParagraph = (CompositeNode)body.AppendChild(new Paragraph(document));

			builder.MoveTo(targetParagraph);

			var field = builder.InsertField(code);

			nodes.Add(field.Start);
			nodes.AddRange(field.Start.GetFollowingSiblings().TakeUntilNode(field.End));
			nodes.Add(field.End);

			targetParagraph.Remove();

			return nodes.ToArray();
		}
	}
}
