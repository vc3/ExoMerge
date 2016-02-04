using System;
using System.Collections.Generic;
using System.Linq;
using Aspose.Words;
using Aspose.Words.Fields;
using ExoMerge.Aspose.Extensions;

namespace ExoMerge.Aspose.Common
{
	/// <summary>
	/// A class that manages the use of a reference field in place of quote characters.
	/// </summary>
	/// <remarks>
	/// This behavior may be needed if injecting quote characters into a document during
	/// the merge process has unindended side-effects, e.g. corrupting IF fields.
	/// </remarks>
	public static class QuoteCharacters
	{
		private const char StandardQuoteCharacter = '"';

		private const char FancyLeftQuoteCharacter = '“';

		private const char FancyRightQuoteCharacter = '”';

		/// <summary>
		/// Enhances the built-in substring method by account for out-of-bounds conditions.
		/// </summary>
		private static string SafeSubstring(string str, int startIndex, int length)
		{
			if (startIndex < 0 || length <= 0 || startIndex >= str.Length)
				return "";

			return str.Substring(startIndex, Math.Min(str.Length - startIndex, length));
		}

		/// <summary>
		/// Gets the container for the quote field: the first paragraph in the document.
		/// </summary>
		private static CompositeNode GetReferenceFieldContainer(Document document)
		{
			return document.SelectNodes("//Body/Paragraph").OfType<Paragraph>().FirstOrDefault();
		}

		/// <summary>
		/// Determines if the given field start is the quote reference field.
		/// </summary>
		public static bool IsQuoteField(FieldStart start)
		{
			FieldEnd end;
			return IsQuoteField(start, out end);
		}

		/// <summary>
		/// Determines if the given field start is the quote reference field and returns its field end if so.
		/// </summary>
		public static bool IsQuoteField(FieldStart start, out FieldEnd end)
		{
			if (start.FieldType != FieldType.FieldSet)
			{
				end = null;
				return false;
			}

			return start.ParseInlineField(out end).Trim() == "SET q \"\\\"\"";
		}

		/// <summary>
		/// Ensure that the quote field is present at the start of the given document.
		/// </summary>
		public static void EnsureQuoteField(Document document)
		{
			var container = GetReferenceFieldContainer(document);
			if (container == null)
				throw new Exception("Unable to place quote reference field: no paragraph could be found.");

			// If the first field in the document is "SET q", then exit early
			var firstField = container.FirstChild as FieldStart;
			if (firstField != null && IsQuoteField(firstField))
				return;

			// Otherwise, insert "SET q"
			var builder = new DocumentBuilder(document);
			if (container.FirstChild != null)
			{
				// Insert the new field at the beginning of the first paragraph
				var field = builder.InsertField(" SET q \"\\\"\" ");
				var insertBefore = container.FirstChild;
				foreach (var node in field.Start.GetSelfAndFollowingSiblings().TakeUpToNode(field.End).Reverse().ToArray())
				{
					container.InsertBefore(node, insertBefore);
					insertBefore = node;
				}
			}
			else
			{
				// The paragraph is empty, so just insert the field
				builder.MoveToParagraph(0, 0);
				builder.InsertField(" SET q \"\\\"\" ");
			}
		}

		/// <summary>
		/// Convert quotes in the given node to instead use the quote field reference.
		/// </summary>
		private static bool ConvertToFieldReferences(Document document, Node node)
		{
			Node[] nodes;
			return ConvertToFieldReferences(document, node, out nodes);
		}

		/// <summary>
		/// Convert quotes in the given node to instead use the quote field reference.
		/// </summary>
		public static bool ConvertToFieldReferences(Document document, Node node, out Node[] nodes)
		{
			var foundQuotes = false;

			if (node is CompositeNode)
			{
				foreach (Node child in ((CompositeNode)node).ChildNodes)
				{
					if (ConvertToFieldReferences(document, child))
						foundQuotes = true;
				}

				nodes = new[] { node };
			}
			else if (node is Run)
			{
				// Replace quotes in a run's text
				if (ConvertToFieldReferences(document, (Run)node, out nodes))
					foundQuotes = true;
			}
			else
			{
				// Since nothing was done, the output is the node that was passed in
				//lastNode = node;
				nodes = new[] { node };
			}

			if (foundQuotes)
				EnsureQuoteField(document);

			return foundQuotes;
		}

		/// <summary>
		/// Attempts to find the next indices of the standard, fancy left, and fancy right quote characters in the given string.
		/// </summary>
		private static bool HasQuotes(string text, int startIndex, out int indexOfStandardQuote, out int indexOfFancyLeftQuote, out int indexOfFancyRightQuote)
		{
			indexOfStandardQuote = text.IndexOf(StandardQuoteCharacter, startIndex);
			indexOfFancyLeftQuote = text.IndexOf(FancyLeftQuoteCharacter, startIndex);
			indexOfFancyRightQuote = text.IndexOf(FancyRightQuoteCharacter, startIndex);

			return indexOfStandardQuote >= 0 || indexOfFancyLeftQuote >= 0 || indexOfFancyRightQuote >= 0;
		}

		/// <summary>
		/// Attempts to find the next indices of the standard, fancy left, and fancy right quote characters in the given string.
		/// </summary>
		private static bool HasQuotes(string text, int startIndex, out int indexOfStandardQuote, out int indexOfFancyQuote)
		{
			int indexOfFancyLeftQuote;
			int indexOfFancyRightQuote;

			if (!HasQuotes(text, startIndex, out indexOfStandardQuote, out indexOfFancyLeftQuote, out indexOfFancyRightQuote))
			{
				indexOfFancyQuote = -1;
				return false;
			}

			if (indexOfFancyLeftQuote == -1)
				indexOfFancyQuote = indexOfFancyRightQuote;
			else if (indexOfFancyRightQuote == -1)
				indexOfFancyQuote = indexOfFancyLeftQuote;
			else
				indexOfFancyQuote = Math.Min(indexOfFancyLeftQuote, indexOfFancyRightQuote);

			return true;
		}

		/// <summary>
		/// Convert quotes in the given node to instead use the quote field reference.
		/// </summary>
		private static bool ConvertToFieldReferences(Document document, Run run, out Node[] nodes)
		{
			var nodeList = new List<Node> { run };

			var foundQuotes = false;

			var currentRun = run;
			var text = run.Text;

			int indexOfStandardQuote;
			int indexOfFancyLeftQuote;
			int indexOfFancyRightQuote;

			while (HasQuotes(text, 0, out indexOfStandardQuote, out indexOfFancyLeftQuote, out indexOfFancyRightQuote))
			{
				foundQuotes = true;

				// Clone the current run
				var newRun = (Run)currentRun.Clone(true);

				nodeList.Add(newRun);

				// Get the index of the first quote character encountered and the character itself
				var index = new[] { indexOfStandardQuote, indexOfFancyLeftQuote, indexOfFancyRightQuote }.Where(i => i >= 0).Min();

				// Split the text
				currentRun.Text = text.Substring(0, index);
				newRun.Text = SafeSubstring(text, index + 1, text.Length);

				var fieldNodes = NodeGenerator.CreateNodesForField(document, " REF q ");

				nodeList.InsertRange(nodeList.IndexOf(newRun), fieldNodes);

				// Find next occurrence
				currentRun = newRun;
				text = newRun.Text;
			}

			nodes = nodeList.ToArray();

			return foundQuotes;
		}

		/// <summary>
		/// Attempts to replace balanced fancy quotes with standard quotes.
		/// </summary>
		public static bool TryReplaceFancyQuotes(string text, out string newText)
		{
			var replacedQuotes = false;

			var result = text;

			var startIndex = 0;

			int indexOfStandardQuote;
			int indexOfFancyQuote;

			var isQuoteOpen = false;
			var lookForFancyQuoteToEnd = false;

			while (HasQuotes(result, startIndex, out indexOfStandardQuote, out indexOfFancyQuote))
			{
				var index = -1;
				var isFancy = false;

				if (indexOfStandardQuote >= 0)
					index = indexOfStandardQuote;

				if (indexOfFancyQuote >= 0 && (index == -1 || indexOfFancyQuote < index))
				{
					index = indexOfFancyQuote;
					isFancy = true;
				}

				if (index < 0)
					break;

				if (isQuoteOpen)
				{
					if (result[index - 1] == '\\' && result[index - 2] != '\\')
					{
						if (index == indexOfStandardQuote)
						{
							// Ignore escaped standard quotes within an open/close quote pair.
						}
						else
						{
							// Unescape escaped fancy quotes.
							result = result.Substring(0, index - 1) + result.Substring(index);

							// Decrement the index to account for the removed escape character.
							index--;
						}
					}
					else if (lookForFancyQuoteToEnd)
					{
						if (index == indexOfStandardQuote)
						{
							// Auto-escape the standard quote.
							result = result.Substring(0, index) + "\\\"" + result.Substring(index + 1);

							// Increment the index by one to account for the added escape character.
							index++;
						}
						else
						{
							isQuoteOpen = false;
							lookForFancyQuoteToEnd = false;

							if (index == result.Length - 1)
							{
								// The expression ends with a fancy right quote.
								result = result.Substring(0, index) + "\"";
								break;
							}

							result = result.Substring(0, index) + "\"" + result.Substring(index + 1);
						}
					}
					else
					{
						if (index == indexOfStandardQuote)
						{
							isQuoteOpen = false;

							if (index == result.Length - 1)
							{
								// The expression ends with a standard quote.
								break;
							}
						}
						else
						{
							// Auto-escape the fancy quote.
							result = result.Substring(0, index) + "\\" + result.Substring(index);

							// Increment the index by one to account for the added escape character.
							index++;
						}
					}
				}
				else
				{
					// If an escaped quote is found outside of an open quote, then exit and don't update the text.
					if (index > 0 && result[index - 1] == '\\' && (index > 1 && result[index - 2] != '\\'))
					{
						newText = null;
						return false;
					}

					if (isFancy)
					{
						if (index != indexOfFancyQuote)
						{
							newText = null;
							return false;
						}

						if (index == result.Length - 1)
						{
							// The expression ends with an open quote, which isn't valid.
							newText = null;
							return false;
						}

						result = result.Substring(0, index) + "\"" + result.Substring(index + 1);
						lookForFancyQuoteToEnd = true;
					}

					replacedQuotes = true;
					isQuoteOpen = true;
				}

				startIndex = index + 1;
			}

			if (isQuoteOpen)
			{
				newText = null;
				return false;
			}

			if (!replacedQuotes)
			{
				newText = null;
				return false;
			}

			newText = result;
			return true;
		}
	}
}
