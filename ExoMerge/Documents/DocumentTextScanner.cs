using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ExoMerge.Analysis;
using ExoMerge.Documents.Extensions;

namespace ExoMerge.Documents
{
	/// <summary>
	/// Scans a document and identifies tokens within the document's text based on the given start
	/// and end token markers, e.g. "{{" and "}}".
	/// </summary>
	/// <typeparam name="TDocument">The type of document to scan.</typeparam>
	/// <typeparam name="TNode">The type that represents nodes in the document.</typeparam>
	/// <remarks>
	/// 
	/// How to handle nested token marker characters, an example...
	/// 
	/// Curly Brackets
	/// ==============
	/// 
	/// https://en.wikipedia.org/wiki/Bracket#Curly_brackets
	/// 
	/// https://en.wikipedia.org/wiki/Poisson_bracket
	/// Distributivity: {f + g, h} = {f, h} + {g, h}
	/// 
	/// https://en.wikipedia.org/wiki/Set_(mathematics)#Describing_sets
	/// Color = {red, green, blue}
	/// 
	/// http://english.stackexchange.com/questions/127892/are-curly-braces-ever-used-in-normal-text-if-not-why-were-they-created
	/// 
	/// "...however, a single, often large, brace is also used to unite several things."
	/// 
	/// https://books.google.co.uk/books?id=eLH9wuP3nc8C
	/// Because Crookes's use of square brackets, minor typographical errors are here indicated in curly brackets {}.
	/// I take this to mean "{sic}"...
	/// 
	/// https://www.scribendi.com/advice/square_brackets_curly_brackets_angle_brackets.en.html
	/// Unless you are a physicist or a highly skilled mathematician, you are unlikely to encounter curly brackets
	/// in your research or reading. Select your pizza topping {pepper, onion, sausage, tomato, feta, anchovies,
	/// bacon, sun-dried tomatoes, chicken, broccoli} and follow me.
	/// 
	/// http://grammar.yourdictionary.com/punctuation/grammar-braces-usage.html
	/// Braces are mostly used in music or poetry. The only use for a brace in writing is when a writer presents or
	/// creates a list of equal choices for a reader or in a number set.
	/// 
	/// http://grammar.yourdictionary.com/punctuation/how-to-use-brackets-in-grammar.html
	/// This mark has extremely limited usage and mostly for poetry or music. An exception to this would be if a
	/// writer wanted to create a list of items that are all equal choices. Otherwise, this punctation mark would not
	/// be used in academic writing.
	/// 
	/// -------------------------------------------------------------------------------------------------------------
	/// 
	/// Brackets Within a Token
	/// -----------------------
	/// 
	/// The use of brackets inside of a token will most likely correspond to its use in programming languages.
	/// 
	/// 1) In syntax, braces are balanced.
	/// 
	/// However, the most common use in syntax is grouping logic, and since expressions are just that, expressions,
	/// they most likely do not leverage the type of syntax where you would need grouping operators.
	/// 
	/// 2) Otherwise, braces may occur within a string literal.
	/// 
	/// Brackets Outside of Tokens
	/// --------------------------
	/// 
	/// It seems the most likely use for brackets in text outside of tokens is to group a list of items, which may
	/// themselves contain tokens, e.g. "this is a set - {{Token1}, {Token2}}".
	/// 
	/// However, it may also be referred to explicitly in text, e.g...
	/// 
	/// 		The brace, {, is most commonly used to denote a type of grouping in mathematics, music, and poetry.
	/// 
	/// A practical example of this scenario that also involves a token:
	/// 
	/// 		Q: What is the meaning of the curly bracket, {, in music notation? {Q31}
	/// 
	/// -------------------------------------------------------------------------------------------------------------
	/// 
	/// It is best to avoid introducing the need to "escape" token characters that occur **outside** of a token,
	/// since this introduces complexity and requires the scanner to concern itself with nodes that it would have
	/// previously been able to simply ignore, and it is also a major change in behavior in that, otherwise, text
	/// outside of a token would never be modified, and text within a token would typically only be modified due to
	/// merge occurring (which would only happen if the token was successfully evaulated as a token expression).
	/// 
	/// For this reason, it seems best to start by considering the inner-most matching token characters to be a token,
	/// and ignore token characters outside of it (whether or not they are balanced/matched). Then, in order to
	/// accommodate token characters *within* a token, an escape character can be used (which could be applied to
	/// the single token characters or all characters in the token sequence(s), e.g...
	/// 
	/// 		\{
	/// 
	/// 		\]\]
	/// 
	/// The backslash character, "\", is a good default choice for escaping.
	/// 
	/// Another possibility is to consider if the braces appear to be within a string literal and not require escaping
	/// in that case. This seems more prone to error, since literal text could contain quote characters as well as the
	/// token character(s), so literal text *could* be misinterpreted as an outer token and string literal, e.g...
	/// 
	/// 		It has been said of the brace, {, "{Quotation3}".
	/// 
	/// On the other hand, braces in a literal string expression may often occur at the edges of a string literal,
	/// and if those braces are not escaped, then the text within them might just happen to be a valid expression
	/// (a string literal) due to the fact that it is completely surrounded by quotes. In this case the user would
	/// not be presented with an error for that particular token, which is not ideal from a usability perspective.
	/// 
	/// </remarks>
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
		/// <param name="escapeCharacter">The character used to escape characters which should not be considered a start or end marker.</param>
		/// <param name="strict">If true, nested or unexpected token characters are not allowed and will result in an exception.</param>
		public DocumentTextScanner(IDocumentAdapter<TDocument, TNode> adapter, string tokenStart, string tokenEnd, char escapeCharacter = '\0', bool strict = false)
		{
			Adapter = adapter;
			TokenStart = tokenStart;
			TokenEnd = tokenEnd;
			EscapeCharacter = escapeCharacter;

			if (escapeCharacter != '\0')
			{
				// QUESTION: How should an 'escaped' marker outside of a token be treated?
				// Using a regex with a negative lookbehind will result in the "escaped" text being ignored.
				// If the escaping only applied *within* a token, then it would be matched and escape character left in the document.
				TokenStartExpr = new Regex(string.Join("", TokenStart.Select(c => "(?<!" + Regex.Escape(escapeCharacter.ToString()) + ")" + Regex.Escape(c.ToString()))), RegexOptions.Compiled | RegexOptions.CultureInvariant);
				TokenEndExpr = new Regex(string.Join("", TokenEnd.Select(c => "(?<!" + Regex.Escape(escapeCharacter.ToString()) + ")" + Regex.Escape(c.ToString()))), RegexOptions.Compiled | RegexOptions.CultureInvariant);
			}
			else
			{
				TokenStartExpr = new Regex(Regex.Escape(TokenStart), RegexOptions.Compiled | RegexOptions.CultureInvariant);
				TokenEndExpr = new Regex(Regex.Escape(TokenEnd), RegexOptions.Compiled | RegexOptions.CultureInvariant);
			}

			if (!strict)
			{
				AllowNestedTokenMarkers = true;
				IgnoreUnexpectedTokenMarkers = true;
			}
		}

		/// <summary>
		/// Gets the adapter that will be used to access and manipulate a document.
		/// </summary>
		public IDocumentAdapter<TDocument, TNode> Adapter { get; private set; }

		/// <summary>
		/// Gets the token start marker text.
		/// </summary>
		public string TokenStart { get; private set; }

		/// <summary>
		/// Gets an expression used to match a token start marker.
		/// </summary>
		private Regex TokenStartExpr { get; set; }

		/// <summary>
		/// Gets the token end marker text.
		/// </summary>
		public string TokenEnd { get; private set; }

		/// <summary>
		/// Gets an expression used to match a token end marker.
		/// </summary>
		private Regex TokenEndExpr { get; set; }

		/// <summary>
		/// Gets the character used to escape characters which should not be considered a start or end marker.
		/// </summary>
		public char EscapeCharacter { get; private set; }

		/// <summary>
		/// Gets a value indicating whether to allow token start and end
		/// markers within a token, as long as they are balanced.
		/// </summary>
		public bool AllowNestedTokenMarkers { get; private set; }

		/// <summary>
		/// Gets a value indicating whether to ignore token start or end
		/// markers that are not part of a balanced pair.
		/// </summary>
		public bool IgnoreUnexpectedTokenMarkers { get; private set; }

		/// <summary>
		/// Skip nested marker character(s) based on custom rules.
		/// </summary>
		protected virtual bool TrySkipNestedMarkers(string text, int fromStartIndex, out int continueIndex)
		{
			continueIndex = 0;
			return false;
		}

		/// <summary>
		/// Attempt to parse the next balanced start and end indices from the given text.
		/// </summary>
		protected virtual bool TryGetNextStartAndEndIndices(string text, int fromIndex, out int startIndex, out int endIndex, out int continueIndex)
		{
			var firstTokenStart = TokenStartExpr.Match(text, fromIndex);

			if (!firstTokenStart.Success)
			{
				var unmatchedTokenEnd = TokenEndExpr.Match(text, fromIndex);

				if (!IgnoreUnexpectedTokenMarkers)
				{
					// If a token start is not found, look for an unbalanced token start.
					if (unmatchedTokenEnd.Success)
						throw new UnbalancedTextTokenException(text, fromIndex, unmatchedTokenEnd.Index + TokenEnd.Length);
				}

				startIndex = -1;
				endIndex = unmatchedTokenEnd.Success ? unmatchedTokenEnd.Index : -1;
				continueIndex = text.Length;
				return false;
			}

			// If an token end precedes the token start, then the token end is in an unexpected location.
			if (!IgnoreUnexpectedTokenMarkers)
			{
				var precedingTokenEnd = TokenEndExpr.Match(text, fromIndex, firstTokenStart.Index - fromIndex);
				if (precedingTokenEnd.Success)
					throw new UnbalancedTextTokenException(text, precedingTokenEnd.Index, firstTokenStart.Index + TokenStart.Length);
			}

			var afterStartMarkerIndex = firstTokenStart.Index + TokenStart.Length;

			var firstTokenEnd = TokenEndExpr.Match(text, afterStartMarkerIndex);

			// If there is not a following token end, then the token start is unexpected.
			if (!firstTokenEnd.Success)
			{
				if (!IgnoreUnexpectedTokenMarkers)
					throw new UnbalancedTextTokenException(text, firstTokenStart.Index, text.Length);

				startIndex = firstTokenStart.Index;
				endIndex = -1;
				continueIndex = text.Length;
				return false;
			}

			var nextTokenStart = TokenStartExpr.Match(text, afterStartMarkerIndex);

			// If the following token end marker is preceded by another token start marker, then attempt
			// to skip nested token marker based on custom logic, or match the inner-most nested markers. 
			if (nextTokenStart.Success && nextTokenStart.Index < firstTokenEnd.Index)
			{
				// If there is a second token start that precedes the token end, then the markers are in some way nested or otherwise unbalanced.
				if (!AllowNestedTokenMarkers)
					throw new UnbalancedTextTokenException(text, firstTokenStart.Index, firstTokenEnd.Index + TokenEnd.Length);

				// It appears that there are "nested tokens", so assume that nested
				// token characters are balanced and consume as many pairs as possible.

				int skipToIndex;
				if (TrySkipNestedMarkers(text, afterStartMarkerIndex, out skipToIndex))
				{
					var nextTokenEnd = TokenEndExpr.Match(text, skipToIndex);
					if (!nextTokenEnd.Success)
					{
						startIndex = firstTokenStart.Index;
						endIndex = -1;
						continueIndex = text.Length;
						return false;
					}

					startIndex = firstTokenStart.Index;
					endIndex = nextTokenEnd.Index;
					continueIndex = nextTokenEnd.Index + TokenEnd.Length;
					return true;
				}

				int indexOfInnerTokenStart;
				int indexOfInnerTokenEnd;
				int innerContinueIndex;

				if (TryGetNextStartAndEndIndices(text, afterStartMarkerIndex, out indexOfInnerTokenStart, out indexOfInnerTokenEnd, out innerContinueIndex))
				{
					startIndex = indexOfInnerTokenStart;
					endIndex = indexOfInnerTokenEnd;
					continueIndex = innerContinueIndex;
					return true;
				}

				startIndex = firstTokenStart.Index;
				endIndex = -1;
				continueIndex = text.Length;
				return false;
			}

			startIndex = firstTokenStart.Index;
			endIndex = firstTokenEnd.Index;
			continueIndex = firstTokenEnd.Index + TokenEnd.Length;
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
				int continueIndex;

				if (!TryGetNextStartAndEndIndices(text, fromIndex, out indexOfTokenStart, out indexOfTokenEnd, out continueIndex))
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

				if (EscapeCharacter != '\0')
				{
					tokenText = tokenText.Replace(EscapeCharacter + TokenStart, TokenStart);
					tokenText = tokenText.Replace(EscapeCharacter + TokenEnd, TokenEnd);
				}

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
					var prependedRun = Adapter.SpliceLeft(runs[startRunIndex], startRunStartIndex - 1);
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
					var appendedRun = Adapter.SpliceRight(runs[endRunIndex], endRunEndIndex + 1);
					runs.Insert(currentRunIndex + 1, appendedRun);
					endRunTextIndex -= endRunEndIndex;
				}

				yield return CreateToken(runs, startRunIndex, startRunTextIndex, endRunIndex, endRunTextIndex, tokenText);

				// Continue scanning after the new token's end marker.
				fromIndex = continueIndex;
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
