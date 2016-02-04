using ExoMerge.Analysis;

namespace ExoMerge.Aspose
{
	/// <summary>
	/// Overrides the keyword token parser to cleanse token text, i.e. replacing "fancy" quotes
	/// before passing on the token text to the underlying keyword token parser.
	/// </summary>
	public class DocumentKeywordTokenParser<TSourceType> : KeywordTokenParser<TSourceType>
	{
		public DocumentKeywordTokenParser()
			: base(caseSensitive: true)
		{
		}

		/// <summary>
		/// Attempt to parse the given text.
		/// </summary>
		protected override bool TryParse(TSourceType sourceType, string text, out TokenType type, out string value)
		{
			// When authoring field expressions, Word may insert fancy "curly" quotes rather than standard quotes. Since the expression parser
			// is unlikely to expect these characeters, this could result in a syntax error. So, replace the outer quotes with standard quotes.
			var cleansedText = DocumentTokenCleanser.Cleanse(text);

			return base.TryParse(sourceType, cleansedText, out type, out value);
		}
	}
}
