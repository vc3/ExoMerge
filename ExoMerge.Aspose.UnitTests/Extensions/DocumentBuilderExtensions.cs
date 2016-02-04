using System;
using System.Collections.Generic;
using System.Linq;
using Aspose.Words;
using Aspose.Words.Fields;
using ExoMerge.Aspose.UnitTests.Helpers;

namespace ExoMerge.Aspose.UnitTests.Extensions
{
	public static class DocumentBuilderExtensions
	{
		public static Run InsertRun(this DocumentBuilder builder, string text)
		{
			var run = new Run(builder.Document, text);
			builder.InsertNode(run);
			return run;
		}

		/// <summary>
		/// Returns an enumeration of the given node and its sibling nodes that
		/// follow the given node in the document.
		/// </summary>
		/// <param name="startingNode">The node to start at.</param>
		/// <returns>An enumeration of nodes.</returns>
		private static IEnumerable<Node> GetNodeAndFollowingNodes(Node startingNode)
		{
			if (startingNode == null)
				throw new ArgumentNullException("startingNode");

			yield return startingNode;

			if (startingNode.NextSibling == null)
				yield break;

			for (var node = startingNode.NextSibling; node != null; node = node.NextSibling)
				yield return node;
		}

		public static Field[] InsertNodesFromText(this DocumentBuilder self, string text)
		{
			var newFields = new List<Field>();

			text = text.Replace("\r\n", "\n");

			var documentHasExistingContent = true;
			var paragraphs = self.Document.SelectNodes("//Body/Paragraph").Cast<Paragraph>().ToArray();
			if (paragraphs.Length == 1 && self.CurrentParagraph == paragraphs[0] && paragraphs[0].ChildNodes.Count == 0)
				documentHasExistingContent = false;

			var pendingField = false;

			var fieldDelimiters = new[] { FieldCharacters.Start, FieldCharacters.Separator, FieldCharacters.End };

			var fieldStack = new Stack<Field>();

			var lines = text.Trim().Split('\n');

			for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++)
			{
				// No need to create a new paragraph if this is a new document
				if (lineIndex > 0 || documentHasExistingContent)
					self.InsertParagraph();

				var line = (lines[lineIndex] ?? "").Trim();

				var startIndex = 0;

				do
				{
					var delimiterIndex = line.IndexOfAny(fieldDelimiters, startIndex);

					if (delimiterIndex == startIndex)
						startIndex = delimiterIndex + 1;
					else
					{
						string runText;
						if (delimiterIndex == -1)
						{
							runText = line.Substring(startIndex);
							startIndex = line.Length;
						}
						else
						{
							runText = line.Substring(startIndex, delimiterIndex - startIndex);
							startIndex = delimiterIndex + 1;
						}

						if (pendingField)
						{
							fieldStack.Push(self.InsertField(runText));
							pendingField = false;
						}
						else
						{
							self.Write(runText);
						}
					}

					if (delimiterIndex != -1)
					{
						var delimiter = line[delimiterIndex];

						switch (delimiter)
						{
							case FieldCharacters.Start:

								if (pendingField)
									throw new Exception("Invalid syntax on line " + lineIndex + " col " + delimiterIndex + ".");

								pendingField = true;

								break;

							case FieldCharacters.Separator:

								throw new Exception("Invalid syntax on line " + lineIndex + " col " + delimiterIndex + ".");

							case FieldCharacters.End:

								if (pendingField || fieldStack.Count == 0)
									throw new Exception("Invalid syntax on line " + lineIndex + " col " + delimiterIndex + ".");

								var endingField = fieldStack.Pop();

								var nodesToMove = GetNodeAndFollowingNodes(endingField.Separator).TakeUpToItem(endingField.End).ToArray();

								var insertAfter = self.CurrentNode ?? self.CurrentParagraph.ChildNodes.Cast<Node>().LastOrDefault();

								foreach (var node in nodesToMove)
								{
									self.CurrentParagraph.InsertAfter(node, insertAfter);
									insertAfter = node;
								}

								endingField.Update();

								newFields.Add(endingField);

								self.MoveTo(endingField.End.ParentParagraph);

								break;

							default:
								throw new Exception("Invalid delimiter '" + delimiter + "' at col " + delimiterIndex + ".");
						}
					}

				} while (startIndex < line.Length);
			}

			if (fieldStack.Count > 0)
				throw new Exception("Unbalanced field syntax.");

			return newFields.ToArray();
		}
	}
}
