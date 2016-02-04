using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExoMerge.Analysis;

namespace ExoMerge.Documents
{
	/// <summary>
	/// Scans a document and identifies tokens within the document's text based on the given start
	/// and end token markers, e.g. "{{" and "}}".
	/// </summary>
	/// <typeparam name="TDocument">The type of document to scan.</typeparam>
	/// <typeparam name="TNode">The type that represents nodes in the document.</typeparam>
	public class DocumentTextScanner<TDocument, TNode> : ITemplateScanner<TDocument, DocumentToken<TNode>>
		where TDocument : class
		where TNode : class
	{
		/// <summary>
		/// Creates a new instance that will identify tokens using the given start and end markers.
		/// </summary>
		/// <param name="adapter">The adapter that will be used to access and manipulate a document.</param>
		/// <param name="tokenStart">The text that marks the start of a token.</param>
		/// <param name="tokenEnd">The text that marks the end of a token.</param>
		/// <param name="strict">If true, nested or unexpected token characters are not allowed and will result in an exception.</param>
		public DocumentTextScanner(IDocumentAdapter<TDocument, TNode> adapter, string tokenStart, string tokenEnd, bool strict = false)
		{
			Adapter = adapter;
			TokenStart = tokenStart;
			TokenEnd = tokenEnd;

			if (!strict)
			{
				AllowNestedTokenCharacters = true;
				IgnoreUnexpectedTokenCharacters = true;
			}
		}

		/// <summary>
		/// Gets the adapter that will be used to access and manipulate a document.
		/// </summary>
		public IDocumentAdapter<TDocument, TNode> Adapter { get; private set; }

		/// <summary>
		/// The token start marker text.
		/// </summary>
		public string TokenStart { get; private set; }

		/// <summary>
		/// The token end marker text.
		/// </summary>
		public string TokenEnd { get; private set; }

		/// <summary>
		/// Gets a value indicating whether to allow token start and end
		/// characters within a token, as long as they are balanced.
		/// </summary>
		public bool AllowNestedTokenCharacters { get; private set; }

		/// <summary>
		/// Gets a value indicating whether to ignore token start or end
		/// characters that are not part of a balanced pair.
		/// </summary>
		public bool IgnoreUnexpectedTokenCharacters { get; private set; }

		/// <summary>
		/// Clones the given run and splits it's text after the given index, assigning the left
		/// portion to the new node and the following text to the existing node.
		/// </summary>
		/// <param name="run">The run to clone/split.</param>
		/// <param name="endIndex">The index of the last character of text to assign to the cloned node.</param>
		/// <returns>The cloned node.</returns>
		protected TNode SpliceLeft(TNode run, int endIndex)
		{
			var prependRun = Adapter.Clone(run, false);

			var text = Adapter.GetText(run);

			Adapter.SetText(prependRun, text.Substring(0, endIndex + 1));
			Adapter.SetText(run, text.Substring(endIndex + 1));

			Adapter.InsertBefore(prependRun, run);

			return prependRun;
		}

		/// <summary>
		/// Clones the given run and splits it's text starting at the given index, assigning the right
		/// portion to the new node and the preceding text to the existing node.
		/// </summary>
		/// <param name="run">The run to clone/split.</param>
		/// <param name="startIndex">The index of the first character of text to assign to the cloned node.</param>
		/// <returns>The cloned node.</returns>
		protected TNode SpliceRight(TNode run, int startIndex)
		{
			var appendRun = Adapter.Clone(run, false);

			var text = Adapter.GetText(run);

			Adapter.SetText(appendRun, text.Substring(startIndex));
			Adapter.SetText(run, text.Substring(0, startIndex));

			Adapter.InsertAfter(appendRun, run);

			return appendRun;
		}

		/// <summary>
		/// Attempt to parse the next balanced start and end indices from the given text.
		/// </summary>
		private bool TryGetNextStartAndEndIndices(string text, int fromIndex, out int startIndex, out int endIndex)
		{
			var indexOfTokenStart = text.IndexOf(TokenStart, fromIndex, StringComparison.Ordinal);

			if (indexOfTokenStart < 0)
			{
				var indexOfUnbalancedTokenEnd = text.IndexOf(TokenEnd, fromIndex, StringComparison.Ordinal);

				if (!IgnoreUnexpectedTokenCharacters)
				{
					// If a token start is not found, look for an unbalanced token start.
					if (indexOfUnbalancedTokenEnd >= 0)
						throw new UnbalancedTextTokenException(text, fromIndex, indexOfUnbalancedTokenEnd + TokenEnd.Length);
				}

				startIndex = indexOfTokenStart;
				endIndex = indexOfUnbalancedTokenEnd;
				return false;
			}

			// If an token end precedes the token start, then the token end is in an unexpected location.
			if (!IgnoreUnexpectedTokenCharacters)
			{
				var indexOfPrecedingTokenEnd = text.IndexOf(TokenEnd, fromIndex, indexOfTokenStart - fromIndex, StringComparison.Ordinal);
				if (indexOfPrecedingTokenEnd != -1)
					throw new UnbalancedTextTokenException(text, indexOfPrecedingTokenEnd, indexOfTokenStart);
			}

			var fromStartIndex = indexOfTokenStart + TokenStart.Length;

			var indexOfTokenEnd = text.IndexOf(TokenEnd, fromStartIndex, StringComparison.Ordinal);
			var indexOfNextTokenStart = text.IndexOf(TokenStart, fromStartIndex, StringComparison.Ordinal);

			if (indexOfNextTokenStart >= 0 && indexOfNextTokenStart < indexOfTokenEnd)
			{
				// If there is a second token start that precedes the token end, then the markers are in some way nested or otherwise unbalanced.
				if (!AllowNestedTokenCharacters)
					throw new UnbalancedTextTokenException(text, indexOfTokenStart, indexOfTokenEnd + TokenEnd.Length);

				// It appears that there are "nested tokens", so assume that nested
				// token characters are balanced and consume as many pairs as possible.

				// TODO: Add escapeability of token start and end character(s)?

				int indexOfInnerTokenStart;
				int indexOfInnerTokenEnd;

				if (!TryGetNextStartAndEndIndices(text, fromStartIndex, out indexOfInnerTokenStart, out indexOfInnerTokenEnd))
				{
					startIndex = indexOfTokenStart;
					endIndex = -1;
					return false;
				}

				indexOfTokenEnd = text.IndexOf(TokenEnd, indexOfInnerTokenEnd + TokenEnd.Length, StringComparison.Ordinal);
			}

			// If there is not a following token end, then the token start is unexpected.
			if (indexOfTokenEnd < 0)
			{
				if (!IgnoreUnexpectedTokenCharacters)
					throw new UnbalancedTextTokenException(text, indexOfTokenStart, text.Length);

				startIndex = indexOfTokenStart;
				endIndex = indexOfTokenEnd;
				return false;
			}

			startIndex = indexOfTokenStart;
			endIndex = indexOfTokenEnd;
			return true;
		}

		/// <summary>
		/// Get a single combined string for the given sequence of runs.
		/// </summary>
		protected virtual string GetText(IList<TNode> runs)
		{
			return runs.Aggregate(new StringBuilder(), (sb, r) => sb.Append(Adapter.GetText(r))).ToString();
		}

		/// <summary>
		/// Gets the text of the run at the given index.
		/// </summary>
		protected virtual string GetText(IList<TNode> runs, int index)
		{
			return Adapter.GetText(runs[index]);
		}

		/// <summary>
		/// Create a document token for the given runs and text.
		/// </summary>
		protected virtual DocumentToken<TNode> CreateToken(IList<TNode> runs, int startRunIndex, int startRunTextIndex, int endRunIndex, int endRunTextIndex, string tokenText)
		{
			return new DocumentToken<TNode>(runs[startRunIndex], runs[endRunIndex], tokenText);
		}

		/// <summary>
		/// Scan the text in the given sequence of runs and returns any tokens that are found.
		/// </summary>
		protected virtual IEnumerable<DocumentToken<TNode>> ScanText(IList<TNode> runs)
		{
			var currentRunIndex = 0;
			var previousRunsTextLength = 0;

			var text = GetText(runs);

			var fromIndex = 0;

			while (fromIndex < text.Length)
			{
				int indexOfTokenStart;
				int indexOfTokenEnd;

				if (!TryGetNextStartAndEndIndices(text, fromIndex, out indexOfTokenStart, out indexOfTokenEnd))
					break;

				// Include the token start and end character(s) in the token.
				var tokenStartIndex = indexOfTokenStart;
				var tokenEndIndex = indexOfTokenEnd + TokenEnd.Length - 1;

				// Parse out the inner contents without the token start and end characters in order to assign the token's value.
				var tokenTextStart = indexOfTokenStart + TokenStart.Length;
				var tokenTextEnd = indexOfTokenEnd - 1;
				var tokenText = text.Substring(tokenTextStart, tokenTextEnd - tokenTextStart + 1);

				// Reject if the indices don't make sense.
				if (tokenStartIndex >= tokenEndIndex)
					throw new Exception(string.Format("Scan discovered invalid text: (\"{0}\", {1}, {2}).", tokenText, tokenStartIndex, tokenEndIndex));

				if (tokenStartIndex < previousRunsTextLength)
				{
					// If for some reason tuples are received out of order, then start over at the beginning.
					previousRunsTextLength = 0;
					currentRunIndex = 0;
				}

				while (currentRunIndex < runs.Count)
				{
					var runText = GetText(runs, currentRunIndex);

					// Skip runs prior to encountering the start of the new token.
					if (runText != null && previousRunsTextLength + runText.Length > tokenStartIndex)
						break;

					if (runText != null)
						previousRunsTextLength += runText.Length;

					currentRunIndex++;
				}

				var startRunIndex = currentRunIndex;

				var startRunTextIndex = previousRunsTextLength;

				var startRunStartIndex = tokenStartIndex - previousRunsTextLength;
				if (startRunStartIndex != 0)
				{
					// If the token text does not start at the beginning of the run's
					// text, then splice off the preceding text into a new run.
					var prependedRun = SpliceLeft(runs[startRunIndex], startRunStartIndex - 1);
					runs.Insert(currentRunIndex, prependedRun);
					previousRunsTextLength += Adapter.GetText(prependedRun).Length;
					startRunTextIndex += startRunStartIndex;
					currentRunIndex++;
					startRunIndex++;
				}

				while (currentRunIndex < runs.Count)
				{
					var runText = GetText(runs, currentRunIndex);

					// Skip runs prior to encountering the end of the new token.
					if (runText != null && previousRunsTextLength + runText.Length > tokenEndIndex)
						break;

					if (runText != null)
						previousRunsTextLength += runText.Length;

					currentRunIndex++;
				}

				var endRunIndex = currentRunIndex;

				var endRunTextIndex = previousRunsTextLength;

				var endRunEndIndex = tokenEndIndex - previousRunsTextLength;
				if (endRunEndIndex != GetText(runs, endRunIndex).Length - 1)
				{
					// If the token text does not end at the end of the run's text,
					// then splice off the following text into a new run.
					var appendedRun = SpliceRight(runs[endRunIndex], endRunEndIndex + 1);
					runs.Insert(currentRunIndex + 1, appendedRun);
					endRunTextIndex -= endRunEndIndex;
				}

				yield return CreateToken(runs, startRunIndex, startRunTextIndex, endRunIndex, endRunTextIndex, tokenText);

				// Continue scanning after the new token's end marker.
				fromIndex = indexOfTokenEnd + TokenEnd.Length;
			}
		}

		/// <summary>
		/// Scan nodes for contiguous sequences of runs and then scan the runs' text for tokens.
		/// </summary>
		private IEnumerable<DocumentToken<TNode>> ScanNodes(IEnumerable<TNode> nodes)
		{
			var sequentialRuns = new List<TNode>();

			foreach (var node in nodes)
			{
				if (Adapter.GetNodeType(node) == DocumentNodeType.Run)
					sequentialRuns.Add(node);
				else if (Adapter.IsNonVisibleMarker(node))
				{
					// Skip over inline nodes that are not visible.
				}
				else
				{
					if (sequentialRuns.Count > 0)
					{
						foreach (var token in ScanText(sequentialRuns))
							yield return token;

						sequentialRuns = new List<TNode>();
					}

					if (Adapter.IsComposite(node))
						foreach (var token in ScanNodes(Adapter.GetChildren(node)))
							yield return token;
				}
			}

			if (sequentialRuns.Count > 0)
			{
				foreach (var token in ScanText(sequentialRuns))
					yield return token;
			}
		}

		public IEnumerable<DocumentToken<TNode>> GetTokens(TDocument template)
		{
			return ScanNodes(Adapter.GetChildren(template));
		}
	}

	/// <summary>
	/// An exception that is thrown when unbalanced token markers are encountered.
	/// </summary>
	public class UnbalancedTextTokenException : Exception
	{
		internal UnbalancedTextTokenException(string text, int startIndex, int endIndex)
			: base(string.Format("Found unbalanced token with text \"{0}\".", text.Substring(startIndex, endIndex - startIndex)))
		{
			Text = text;
			StartIndex = startIndex;
			EndIndex = endIndex;
		}

		/// <summary>
		/// The text block that was being scanned.
		/// </summary>
		public string Text { get; private set; }

		/// <summary>
		/// The index where the unbalanced text may start.
		/// </summary>
		public int StartIndex { get; private set; }

		/// <summary>
		/// The index where the unbalanced text may end.
		/// </summary>
		public int EndIndex { get; private set; }
	}
}
