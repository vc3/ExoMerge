using System.Collections.Generic;

namespace ExoMerge.Analysis
{
	/// <summary>
	/// An object that parses and extracts key/value options from an expression.
	/// </summary>
	public interface IOptionParser
	{
		/// <summary>
		/// Extract options from the given text and return the remainder
		/// of the text that doesn't pertain to the extracted options.
		/// </summary>
		/// <param name="text">The text to extract options from.</param>
		/// <param name="options">The extracted options.</param>
		/// <returns>The remaining portion of the expression.</returns>
		string ExtractOptions(string text, out KeyValuePair<string, string>[] options);
	}
}
