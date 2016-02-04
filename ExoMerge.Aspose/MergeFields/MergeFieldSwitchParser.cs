using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using ExoMerge.Analysis;

namespace ExoMerge.Aspose.MergeFields
{
	/// <summary>
	/// Parses trailing switches from a MERGEFIELD's code.
	/// </summary>
	public class MergeFieldSwitchParser : IOptionParser
	{
		private static readonly Regex SwitchExpr = new Regex(@"(?<=\s)\\(?<key>[^\\\s]+)\s+(?<value>[^\\]+)(?=(?:$|\\))", RegexOptions.Compiled);

		private static readonly Regex SurroundingQuotesExpr = new Regex("^\\s*\"(.+)\"\\s*$", RegexOptions.Compiled);

		/// <summary>
		/// Creates a new merge field switch parser that will extract any switches that are found.
		/// </summary>
		public MergeFieldSwitchParser()
		{
		}

		/// <summary>
		/// Creates a new merge field switch parser that will extract only the given switches.
		/// </summary>
		/// <param name="switchKeys">The switches to extract.</param>
		public MergeFieldSwitchParser(string[] switchKeys)
		{
			SwitchKeys = switchKeys;
		}

		/// <summary>
		/// The switches that should be parsed.
		/// </summary>
		public string[] SwitchKeys { get; private set; }

		/// <summary>
		/// Whether or not to skip over unknown switches if they are encountered.
		/// </summary>
		public bool SkipOverUnknownSwitches { get; set; }

		/// <summary>
		/// Remove surrounding quotes from the given string.
		/// </summary>
		internal static string RemoveSurroundingQuotes(string value)
		{
			if (SurroundingQuotesExpr.IsMatch(value))
				return SurroundingQuotesExpr.Replace(value, "$1");

			return value;
		}

		/// <summary>
		/// Extract switches from the given MERGEFIELD code and return the remainder
		/// of the code that doesn't pertain to the extracted switches.
		/// </summary>
		/// <param name="code">The MERGEFIELD code to extract switches from.</param>
		/// <param name="switches">The extracted switches.</param>
		/// <returns>The remaining portion of the MERGEFIELD code.</returns>
		public string ExtractOptions(string code, out KeyValuePair<string, string>[] switches)
		{
			var switchList = new List<KeyValuePair<string, string>>();

			var matches = SwitchExpr.Matches(code);

			Match lastMatch = null;

			for (var i = matches.Count - 1; i >= 0; i--)
			{
				var match = matches[i];

				var key = match.Groups["key"].Value;

				if (SwitchKeys != null && !SwitchKeys.Contains(key))
				{
					if (SkipOverUnknownSwitches)
						continue;

					break;
				}

				var value = RemoveSurroundingQuotes(match.Groups["value"].Value.Trim());

				switchList.Insert(0, new KeyValuePair<string, string>(key, value));

				lastMatch = match;
			}

			if (lastMatch == null)
			{
				switches = new KeyValuePair<string, string>[0];
				return code;
			}

			switches = switchList.ToArray();
			return code.Substring(0, lastMatch.Index).Trim();
		}
	}
}
