using System;
using System.Collections.Generic;
using System.Text;
using Aspose.Words;
using Aspose.Words.Fields;
using ExoMerge.Aspose.Common;
using ExoMerge.Documents;
using ExoMerge.Documents.Extensions;

namespace ExoMerge.Aspose
{
	/// <summary>
	/// Scans a document and identifies tokens within the document's text based on the given start
	/// and end token markers, e.g. "{{" and "}}".
	/// </summary>
	public class DocumentTextScanner : DocumentTextScanner<Document, Node>
	{
		/// <summary>
		/// Creates a new instance that will identify tokens using the given start and end markers.
		/// </summary>
		public DocumentTextScanner(IDocumentAdapter<Document, Node> adapter, string tokenStart, string tokenEnd, char escapeCharacter = '\0', bool strict = false)
			: base(adapter, tokenStart, tokenEnd, escapeCharacter, strict)
		{
		}

		/// <summary>
		/// Determine if the set of runs corresponds to the text of a HYPERLINK field, in which case the
		/// text may have been URI-encoded by Word, so we need to unencode it in order to discover tokens.
		/// </summary>
		private static bool TryParseHyperlink(IList<Node> runs, out int valueStart, out int valueEnd)
		{
			var precedingFieldStart = runs[0].PreviousSibling as FieldStart;

			if (precedingFieldStart != null)
			{
				HyperlinkField hyperlinkField;
				if (HyperlinkField.TryParse(precedingFieldStart, out hyperlinkField))
				{
					try
					{
						return hyperlinkField.GetValue(out valueStart, out valueEnd) != null;
					}
					catch
					{
						// ignored
					}
				}
			}

			valueStart = -1;
			valueEnd = -1;
			return false;
		}

		/// <summary>
		/// Enumerate a list of text runs.
		/// </summary>
		private static void EnumerateRuns(IList<Node> runs, Func<int, string, int, bool> step)
		{
			var currentRunIndex = 0;
			var previousRunsTextLength = 0;

			while (currentRunIndex < runs.Count)
			{
				if (((Run)runs[currentRunIndex]).Text != null)
				{
					if (!step(currentRunIndex, ((Run)runs[currentRunIndex]).Text, previousRunsTextLength))
						return;

					previousRunsTextLength += ((Run)runs[currentRunIndex]).Text.Length;
				}

				currentRunIndex++;
			}
		}

		/// <summary>
		/// Scan the text in the given sequence of runs and returns any tokens that are found.
		/// </summary>
		protected override IEnumerable<DocumentToken<Node>> ScanText(IList<Node> runs)
		{
			int valueStart;
			int valueEnd;

			if (TryParseHyperlink(runs, out valueStart, out valueEnd))
			{
				// Ensure that the hyperlink value is in one or more runs by itself,
				// so that it can more easily be targetted for encoding/decoding.

				EnumerateRuns(runs, (currentRunIndex, currentRunText, previousRunsTextLength) =>
				{
					// If the run overlaps with the value start index, then split the run in two.
					if (previousRunsTextLength < valueStart + 1 && previousRunsTextLength + currentRunText.Length > valueStart)
					{
						runs.Insert(currentRunIndex + 1, Adapter.SpliceRight(runs[currentRunIndex], valueStart - previousRunsTextLength));

						// Update vars to reflect the fact that the run was split.
						previousRunsTextLength += valueStart - previousRunsTextLength;
						currentRunIndex++;
					}

					// If the run overlaps with the value end index, then split the run in two.
					if (previousRunsTextLength < valueEnd + 1 && previousRunsTextLength + currentRunText.Length - 1 > valueEnd)
						runs.Insert(currentRunIndex, Adapter.SpliceLeft(runs[currentRunIndex], valueEnd - previousRunsTextLength));

					// No need to continue if we've gone beyond hte value portion of the hyperlink.
					return previousRunsTextLength + currentRunText.Length < valueEnd;
				});
			}

			return base.ScanText(runs);
		}

		protected override DocumentToken<Node> CreateToken(IList<Node> runs, int startRunIndex, int startRunTextIndex, int endRunIndex, int endRunTextIndex, string tokenText)
		{
			var encoding = DocumentTextEncoding.None;

			int valueStart;
			int valueEnd;

			if (TryParseHyperlink(runs, out valueStart, out valueEnd))
			{
				if (startRunTextIndex == valueStart)
				{
					// If the token is at the beginning of the hyperlink value, then assume that it should represent a non-escaped URL.
					encoding = DocumentTextEncoding.None;
				}
				else
				{
					// This is an oversimplification, but for now consider a token that is not at the beginning of the hyperlink
					// value to correspond to a portion of the URL that should be URI encoded (i.e. part of the query string).
					// This may not be accurate if the user is using fields to dynamically build the host/path portion of a URL.
					// It may be more appropriate to attempt to parse the hyperlink value as a URI and determine where in the URI
					// the token portion falls and encode if it is part of the query, and not otherwise. However, it may not always
					// be possible to parse the URI (e.g. if fields are used to build the host/path portion of the URI).
					encoding = DocumentTextEncoding.Uri;
				}
			}

			return new DocumentToken<Node>(runs[startRunIndex], runs[endRunIndex], tokenText, encoding);
		}

		protected override string GetText(IList<Node> runs)
		{
			int valueStart;
			int valueEnd;

			// Detect runs within a hyperlink filed in order to account for URI escaping.
			if (TryParseHyperlink(runs, out valueStart, out valueEnd))
			{
				var result = new StringBuilder();

				EnumerateRuns(runs, (currentRunIndex, currentRunText, previousRunsTextLength) =>
				{
					// If the run falls within the hyperlink value, then unescape the value. Note that this will
					// only affect the discovery of field tokens, since the run's text is not actually manipulated.
					// If unescaping results in all or part of a hyperlink value being parsed as a token, then that
					// portion of the text will either be removed from the document (if it is part of a region), or
					// it will be replaced with dynamic content, and *potentially* re-encoded, during merge.
					if (previousRunsTextLength + currentRunText.Length > valueStart && previousRunsTextLength <= valueEnd)
						result.Append(Uri.UnescapeDataString(currentRunText));
					else
						result.Append(currentRunText);

					return true;
				});

				return result.ToString();
			}

			return base.GetText(runs);
		}

		/// <summary>
		/// Gets the text of the run at the given index.
		/// </summary>
		protected override string GetText(IList<Node> runs, int index)
		{
			int valueStart;
			int valueEnd;

			// Detect runs within a hyperlink filed in order to account for URI escaping.
			if (TryParseHyperlink(runs, out valueStart, out valueEnd))
			{
				string result = null;

				EnumerateRuns(runs, (currentRunIndex, currentRunText, previousRunsTextLength) =>
				{
					if (currentRunIndex == index)
					{
						// If the run falls within the hyperlink value, then unescape the value. Note that this will
						// only affect the discovery of field tokens, since the run's text is not actually manipulated.
						// If unescaping results in all or part of a hyperlink value being parsed as a token, then that
						// portion of the text will either be removed from the document (if it is part of a region), or
						// it will be replaced with dynamic content, and *potentially* re-encoded, during merge.
						if (previousRunsTextLength + currentRunText.Length > valueStart && previousRunsTextLength <= valueEnd)
							result = Uri.UnescapeDataString(currentRunText);
						else
							result = currentRunText;
						return false;
					}

					return true;
				});

				return result;
			}

			return base.GetText(runs, index);
		}
	}
}
