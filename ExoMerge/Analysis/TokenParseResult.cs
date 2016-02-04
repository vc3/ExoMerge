using System.Collections.Generic;

namespace ExoMerge.Analysis
{
	/// <summary>
	/// A class that represents the result of parsing a token's text.
	/// </summary>
	public class TokenParseResult
	{
		/// <summary>
		/// Constructs a new instance with the given arguments.
		/// </summary>
		/// <param name="type">The type of the parsed token.</param>
		/// <param name="value">The parsed token value.</param>
		/// <param name="options">The parsed options.</param>
		public TokenParseResult(TokenType type, string value, KeyValuePair<string, string>[] options)
		{
			Type = type;
			Value = value;
			Options = options;
		}

		/// <summary>
		/// Gets the type of the parsed token.
		/// </summary>
		public TokenType Type { get; private set; }

		/// <summary>
		/// Gets the parsed token value.
		/// </summary>
		public string Value { get; private set; }

		/// <summary>
		/// Gets the parsed format specifier.
		/// </summary>
		public KeyValuePair<string, string>[] Options { get; private set; }
	}
}
