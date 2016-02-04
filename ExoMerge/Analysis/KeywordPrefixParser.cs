using System.Linq;
using System.Text.RegularExpressions;

namespace ExoMerge.Analysis
{
	/// <summary>
	/// A class responsible for parsing a keyword prefix from given text.
	/// </summary>
	public class KeywordPrefixParser
	{
		private static readonly Regex Parser = new Regex("^\\s*(?<keyword>[A-Za-z_][A-Za-z0-9_]*)(?:$|(?:\\s+(?<remainder>.*)))$", RegexOptions.IgnoreCase);

		public KeywordPrefixParser()
		{
		}

		public KeywordPrefixParser(string[] keywords)
		{
			Keywords = keywords;
		}

		public string[] Keywords { get; private set; }

		/// <summary>
		/// Attempt to parse the given text and return the keyword prefix and remainder.
		/// </summary>
		/// <param name="text">The text to parse.</param>
		/// <param name="keyword">The keyword prefix.</param>
		/// <param name="remainder">The remainder text.</param>
		/// <returns>A boolean value indicating whether the parse was successful.</returns>
		public bool TryParse(string text, out string keyword, out string remainder)
		{
			var match = Parser.Match(text);

			if (!match.Success)
			{
				keyword = null;
				remainder = null;
				return false;
			}

			keyword = match.Groups["keyword"].Value;

			if (Keywords != null && !Keywords.Contains(keyword))
			{
				keyword = null;
				remainder = null;
				return false;
			}

			remainder = match.Groups["remainder"].Value;

			return true;
		}
	}
}
