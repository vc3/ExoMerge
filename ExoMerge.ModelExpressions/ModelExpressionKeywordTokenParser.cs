using System.Collections.Generic;
using System.Text.RegularExpressions;
using ExoMerge.Analysis;
using ExoModel;

namespace ExoMerge.ModelExpressions
{
	/// <summary>
	/// Overrides the keyword token parser to first ensure that the token is not a
	/// content expression that uses the "if...then...else" ternary syntax. 
	/// </summary>
	public class ModelExpressionKeywordTokenParser : KeywordTokenParser<ModelType>
	{
		private static readonly Regex IfThenElseExpr = new Regex("^\\s*if\\s*.*\\s*then\\s*.*\\s*else\\s*", RegexOptions.Compiled);

		protected ModelExpressionKeywordTokenParser(IOptionParser optionParser = null, bool caseSensitive = false)
			: base(optionParser, caseSensitive)
		{
		}

		/// <summary>
		/// Cleanse the given token value if needed, so that
		/// unexpected characters don't result in syntax errors.
		/// </summary>
		protected virtual string CleanseValue(string tokenValue)
		{
			return tokenValue;
		}

		/// <summary>
		/// Attempt to parse the given token value.
		/// </summary>
		public override TokenParseResult Parse(ModelType sourceType, string tokenValue)
		{
			// Expression text is essentially treated as literal code, so any unexpected characters would result in a syntax error.
			var cleansedTokenValue = CleanseValue(tokenValue);

			KeyValuePair<string, string>[] options;

			// Parse options first, since we must attempt to parse the token value as an expression
			// in order to distinguish an 'if...then...else' ternary expression from the 'if' keyword.
			var text = OptionParser.ExtractOptions(cleansedTokenValue, out options);

			// Look for an 'if ... then ... else' expression.
			if (sourceType != null && !string.IsNullOrEmpty(text) && IfThenElseExpr.IsMatch(text))
			{
				// The token value can be parsed as an expression if it uses the if/then/else ternary syntax.
				// This must be attempted first, since otherwise it would be incorrectly identified as an 'if' block.
				try
				{
					ModelExpression parsedExpression;
					if (sourceType.TryGetExpression(null, text, out parsedExpression))
						return new TokenParseResult(TokenType.ContentField, text, options);
				}
				catch
				{
				}
			}

			TokenType type;
			string value;
			if (!TryParse(sourceType, text, out type, out value))
				return null;

			return new TokenParseResult(type, value != null ? value.Trim() : null, options);
		}
	}
}
