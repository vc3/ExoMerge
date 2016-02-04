using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExoMerge.Analysis
{
	/// <summary>
	/// Parses tokens as repeatable and conditional blocks by looking for the presence of keyword prefixes,
	/// e.g. "if, else if, else & end if", and "each & end each", followed by an expression where appropriate.
	/// </summary>
	/// <remarks>
	/// To keep the syntax simple, punctuation such as parenthesis or curly braces are not used.
	/// Also, an option parser can be provided in the constructor, or the 'ExtractOptions' method
	/// can be override, otherwise it is assumed that the syntax does not support options.
	/// </remarks>
	/// <typeparam name="TSourceType">The type of the source type, e.g. 'Type'.</typeparam>
	public abstract class KeywordTokenParser<TSourceType> : ITokenParser<TSourceType>
	{
		private readonly Regex repeatableBeginExpression;

		private readonly Regex repeatableEndExpression;

		private readonly Regex conditionalBeginExpression;

		private readonly Regex conditionalAlternativeExpression;

		private readonly Regex conditionalDefaultExpression;

		private readonly Regex conditionalEndExpression;

		protected KeywordTokenParser(IOptionParser optionParser = null, bool caseSensitive = false)
			: this(caseSensitive, "each", "end each", "if", "else if", "else", "end if")
		{
			OptionParser = optionParser;
		}

		private KeywordTokenParser(bool caseSensitive, string repeatableBeginKeywords, string repeatableEndKeywords, string ifBeginKeywords, string elseIfKeywords, string elseKeywords, string ifEndKeywords)
		{
			repeatableBeginExpression = GetRegexForKeywords(repeatableBeginKeywords, caseSensitive);
			repeatableEndExpression = GetRegexForKeywords(repeatableEndKeywords, caseSensitive);
			conditionalBeginExpression = GetRegexForKeywords(ifBeginKeywords, caseSensitive);
			conditionalAlternativeExpression = GetRegexForKeywords(elseIfKeywords, caseSensitive);
			conditionalDefaultExpression = GetRegexForKeywords(elseKeywords, caseSensitive);
			conditionalEndExpression = GetRegexForKeywords(ifEndKeywords, caseSensitive);
		}

		/// <summary>
		/// Gets the object used to extract options from a token's text.
		/// </summary>
		public IOptionParser OptionParser { get; private set; }

		/// <summary>
		/// Returns a regular expression that matches the given literal keyword(s) text.
		/// </summary>
		private static Regex GetRegexForKeywords(string text, bool caseSensitive = false)
		{
			var keywords = Regex.Split(text.Trim(), "\\s+");
			var keywordsExpression = string.Join("\\s+", keywords.Select(Regex.Escape));
			var keywordOptions = caseSensitive ? RegexOptions.Compiled : RegexOptions.Compiled | RegexOptions.IgnoreCase;
			return new Regex("^\\s*" + keywordsExpression + "(?=\\s|$)(?<remainder>.*)$", keywordOptions);
		}

		/// <summary>
		/// Extract options from the given text and return the remainder
		/// of the text that doesn't pertain to the extracted options.
		/// </summary>
		protected virtual string ExtractOptions(string value, out KeyValuePair<string, string>[] options)
		{
			if (OptionParser != null)
				return OptionParser.ExtractOptions(value, out options);

			options = null;
			return value;
		}

		/// <summary>
		/// Attempt to parse the given text.
		/// </summary>
		/// <param name="sourceType">The data source type.</param>
		/// <param name="text">The text to parse.</param>
		/// <param name="type">The type of token.</param>
		/// <param name="remainder">The remaining text to parse.</param>
		/// <returns>Whether or not the token text was parsed was successfully.</returns>
		protected virtual bool TryParse(TSourceType sourceType, string text, out TokenType type, out string remainder)
		{
			if (repeatableBeginExpression.IsMatch(text))
			{
				remainder = repeatableBeginExpression.Match(text).Groups["remainder"].Value;
				type = TokenType.RepeatableBegin;
				return true;
			}

			if (repeatableEndExpression.IsMatch(text))
			{
				remainder = repeatableEndExpression.Match(text).Groups["remainder"].Value;
				type = TokenType.RepeatableEnd;
				return true;
			}

			if (conditionalBeginExpression.IsMatch(text))
			{
				remainder = conditionalBeginExpression.Match(text).Groups["remainder"].Value;
				type = TokenType.ConditionalBeginWithTest;
				return true;
			}

			if (conditionalAlternativeExpression.IsMatch(text))
			{
				remainder = conditionalAlternativeExpression.Match(text).Groups["remainder"].Value;
				type = TokenType.ConditionalTest;
				return true;
			}

			if (conditionalDefaultExpression.IsMatch(text))
			{
				remainder = conditionalDefaultExpression.Match(text).Groups["remainder"].Value;
				type = TokenType.ConditionalDefault;
				return true;
			}

			if (conditionalEndExpression.IsMatch(text))
			{
				remainder = conditionalEndExpression.Match(text).Groups["remainder"].Value;
				type = TokenType.ConditionalEnd;
				return true;
			}

			remainder = text;
			type = TokenType.ContentField;
			return true;
		}

		/// <summary>
		/// Attempt to parse the given token value.
		/// </summary>
		/// <param name="sourceType">The data source type.</param>
		/// <param name="tokenValue">The token value to parse.</param>
		/// <returns>The parse result.</returns>
		public virtual TokenParseResult Parse(TSourceType sourceType, string tokenValue)
		{
			TokenType type;
			string remainder;
			if (!TryParse(sourceType, tokenValue, out type, out remainder))
				return null;

			KeyValuePair<string, string>[] options;
			var value = ExtractOptions(remainder, out options);

			return new TokenParseResult(type, value != null ? value.Trim() : null, options);
		}
	}
}
