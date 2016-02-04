using ExoMerge.Aspose.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.Aspose.UnitTests
{
	[TestClass]
	public class QuoteCharactersTests
	{
		[TestMethod]
		public void TryReplaceFancyQuotes_TextInFancyQuotes_Replaced()
		{
			string newText;

			Assert.IsTrue(QuoteCharacters.TryReplaceFancyQuotes("“quoted”", out newText));
			Assert.AreEqual("\"quoted\"", newText);
		}

		[TestMethod]
		public void TryReplaceFancyQuotes_StandardQuoteInFancyQuotes_ReplacedAndEscaped()
		{
			string newText;

			Assert.IsTrue(QuoteCharacters.TryReplaceFancyQuotes("“Standard Quote: \"”", out newText));
			Assert.AreEqual("\"Standard Quote: \\\"\"", newText);
		}

		[TestMethod]
		public void TryReplaceFancyQuotes_EscapedStandardQuoteInFancyQuotes_Replaced()
		{
			string newText;

			Assert.IsTrue(QuoteCharacters.TryReplaceFancyQuotes("“Standard Quote: \\\"”", out newText));
			Assert.AreEqual("\"Standard Quote: \\\"\"", newText);
		}

		[TestMethod]
		public void TryReplaceFancyQuotes_UnescapedFancyQuoteInFancyQuotes_NoAction()
		{
			string newText;

			Assert.IsFalse(QuoteCharacters.TryReplaceFancyQuotes("“Fancy Left Quote: “”", out newText));
			Assert.IsNull(newText);

			Assert.IsFalse(QuoteCharacters.TryReplaceFancyQuotes("“Fancy Right Quote: ””", out newText));
			Assert.IsNull(newText);
		}

		[TestMethod]
		public void TryReplaceFancyQuotes_NestedFancyQuotesAsAutoCorrectedByWord_NoAction()
		{
			// Word auto-corrects to open and closed fancy quotes, but it appears that any quotes after the
			// first one that is typed are inserted as a fancy right quote if there is no whitespace.

			string newText;

			Assert.IsTrue(QuoteCharacters.TryReplaceFancyQuotes("“”unquoted””", out newText));
			Assert.AreEqual("\"\"unquoted\"\"", newText);
		}

		[TestMethod]
		public void TryReplaceFancyQuotes_EscapedFancyQuoteInFancyQuotes_ReplacedAndUnescaped()
		{
			string newText;

			Assert.IsTrue(QuoteCharacters.TryReplaceFancyQuotes("“Fancy Left Quote: \\“”", out newText));
			Assert.AreEqual("\"Fancy Left Quote: “\"", newText);

			Assert.IsTrue(QuoteCharacters.TryReplaceFancyQuotes("“Fancy Right Quote: \\””", out newText));
			Assert.AreEqual("\"Fancy Right Quote: ”\"", newText);
		}

		[TestMethod]
		public void TryReplaceFancyQuotes_UnbalancedFancyQuote_NoAction()
		{
			string newText;

			Assert.IsFalse(QuoteCharacters.TryReplaceFancyQuotes("“unbalanced", out newText));
			Assert.IsNull(newText);
		}

		[TestMethod]
		public void TryReplaceFancyQuotes_OnlyLeftFancyQuotes_Replaced()
		{
			string newText;

			Assert.IsTrue(QuoteCharacters.TryReplaceFancyQuotes(" A + “, “ + B + “, “ + C ", out newText));
			Assert.AreEqual(" A + \", \" + B + \", \" + C ", newText);
		}

		[TestMethod]
		public void TryReplaceFancyQuotes_OneLeftThenOnlyRightFancyQuotesDueToWhitespace_Replaced()
		{
			string newText;

			Assert.IsTrue(QuoteCharacters.TryReplaceFancyQuotes(" “,”+”nowhitespace” ", out newText));
			Assert.AreEqual(" \",\"+\"nowhitespace\" ", newText);
		}
	}
}
