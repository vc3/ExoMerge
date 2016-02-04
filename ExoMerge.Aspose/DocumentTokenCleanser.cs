using ExoMerge.Aspose.Common;

namespace ExoMerge.Aspose
{
	/// <summary>
	/// A class that provides options for cleansing document tokens.
	/// </summary>
	public static class DocumentTokenCleanser
	{
		/// <summary>
		/// Cleanse the given raw text by replacing MS Word fancy quote characters
		/// with standard quote characters, as well as auto-escaping as needed.
		/// </summary>
		/// <remarks>
		/// This is needed because Word by default doesn't make it easy or intuitive to insert
		/// standard quote characters. Since the outer quotes in an expression's text are likely
		/// expected to be standard quotes, it may be preferable to automatically convert them.
		/// If unescaped standard quotes are used within fancy quotes, they are auto-escaped.
		/// Escaped fancy quotes are automatically unescaped. The end result is that quotes
		/// only have to be explicitly escaped if they are enclosed within a pair of the
		/// same kind of quotes.
		/// </remarks>
		/// <param name="tokenValue">The raw token value.</param>
		public static string Cleanse(string tokenValue)
		{
			if (string.IsNullOrEmpty(tokenValue))
				return tokenValue;

			string newValue;
			if (QuoteCharacters.TryReplaceFancyQuotes(tokenValue, out newValue))
				return newValue;

			return tokenValue;
		}
	}
}
