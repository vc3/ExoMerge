using System.Linq;
using Aspose.Words;
using ExoMerge.Analysis;
using ExoMerge.Aspose.UnitTests.Helpers;
using ExoMerge.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.Aspose.UnitTests
{
	public partial class NodeTextScannerTests
	{
		private static void TestScan(string text, ITemplateScanner<Document, DocumentToken<Node>> scanner, params string[] expectedTokens)
		{
			var doc = DocumentConverter.FromStrings(new [] { text });

			var tokens = scanner.GetTokens(doc).ToArray();
	
			Assert.AreEqual(string.Join("\r\n", expectedTokens), string.Join("\r\n", tokens.Select(t => t.Value)));

			Assert.AreEqual(expectedTokens.Length, tokens.Length, "There should be " + expectedTokens.Length + " token" + (expectedTokens.Length == 1 ? "" : "s") + ".");

			for(var i = 0; i < expectedTokens.Length; i++)
				Assert.AreEqual(expectedTokens[i], tokens[i].Value);
		}
	}
}