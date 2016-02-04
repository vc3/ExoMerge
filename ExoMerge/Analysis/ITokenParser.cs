namespace ExoMerge.Analysis
{
	/// <summary>
	/// Parses a token's text into its component parts.
	/// </summary>
	public interface ITokenParser<in TSourceType>
	{
		/// <summary>
		/// Parse the given token text and return its raw component parts.
		/// </summary>
		/// <remarks>
		/// An implementing class should make a "best effort" to parse the given text and return as much
		/// information as it can.
		/// 
		/// ## Null Return Value
		/// 
		/// * If the parser can determine with confidence that the given text is not actually a token, then it should
		///   return null to indicate that to the caller.
		/// 
		/// ## Exceptions
		/// 
		/// * The parser is likely only doing string-based parsing, so it is unlikely that a meaningful exception occurs outside
		///   of the parser. However, if it does, the parser should not catch the exception unless it is able to provide better
		///   context about the exception than down-stream code would be able to derive from the raw exception. The merge
		///   template compiler tracks exceptions and allows a caller to handle them as it sees fit.
		/// 
		/// </remarks>
		/// <param name="sourceType">The data source type.</param>
		/// <param name="tokenValue">The token value to parse.</param>
		/// <returns>Whether or not the token text was parsed successfully.</returns>
		TokenParseResult Parse(TSourceType sourceType, string tokenValue);
	}
}
