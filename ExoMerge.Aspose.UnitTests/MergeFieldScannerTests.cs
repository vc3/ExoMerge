using System.Linq;
using Aspose.Words;
using ExoMerge.Analysis;
using ExoMerge.Aspose.MergeFields;
using ExoMerge.Aspose.UnitTests.Helpers;
using ExoMerge.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.Aspose.UnitTests
{
	[TestClass]
	public class MergeFieldScannerTests
	{
		private static readonly ITemplateScanner<Document, DocumentToken<Node>> Scanner = new MergeFieldScanner();

		[TestMethod]
		public void GetTokens_SingleField_SingleToken()
		{
			var doc = DocumentConverter.FromDisplayCode("{ MERGEFIELD Value }");

			Assert.AreEqual("Value", string.Join(",", Scanner.GetTokens(doc).Select(t => t.Value)));
		}

		[TestMethod]
		public void GetTokens_MultipleFieldsInParagraph_MultipleTokens()
		{
			var doc = DocumentConverter.FromDisplayCode("My favorite colors are { MERGEFIELD Color1 }, { MERGEFIELD Color2 }, and { MERGEFIELD Color3 }.");

			Assert.AreEqual("Color1,Color2,Color3", string.Join(",", Scanner.GetTokens(doc).Select(t => t.Value)));
		}

		[TestMethod]
		public void GetTokens_MultipleFieldsInMultipleParagraphs_MultipleTokens()
		{
			var doc = DocumentConverter.FromDisplayCode(@"
					My favorite colors are { MERGEFIELD Color1 }, { MERGEFIELD Color2 }, and { MERGEFIELD Color3 }.
					My favorite foods are { MERGEFIELD Food1 }, { MERGEFIELD Food2 }, and { MERGEFIELD Food3 }.
				");

			Assert.AreEqual("Color1,Color2,Color3,Food1,Food2,Food3", string.Join(",", Scanner.GetTokens(doc).Select(t => t.Value)));
		}
	}
}
