using System.Collections.Generic;

namespace ExoMerge.Analysis
{
	/// <summary>
	/// Scans a template and produces a sequence of tokens.
	/// </summary>
	/// <typeparam name="TTemplate">The type of template to scan.</typeparam>
	/// <typeparam name="TToken">The type that represents a token.</typeparam>
	public interface ITemplateScanner<in TTemplate, out TToken>
	{
		/// <summary>
		/// Scans the given template template and produces the tokens
		/// that are found, in the order that they occur in the template.
		/// </summary>
		/// <param name="template">The template to scan.</param>
		/// <returns>An ordered enumerable of tokens.</returns>
		IEnumerable<TToken> GetTokens(TTemplate template);
	}
}
