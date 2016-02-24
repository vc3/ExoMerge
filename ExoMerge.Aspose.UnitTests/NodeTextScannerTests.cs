using System.Linq;
using Aspose.Words;
using ExoMerge.Analysis;
using ExoMerge.Aspose.UnitTests.Extensions;
using ExoMerge.Aspose.UnitTests.Helpers;
using ExoMerge.Documents;
using ExoMerge.UnitTests.Assertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.Aspose.UnitTests
{
	[TestClass]
	public partial class NodeTextScannerTests
	{
		private static readonly IDocumentAdapter<Document, Node> Adapter = new DocumentAdapter();

		private static readonly ITemplateScanner<Document, DocumentToken<Node>> LooseScanner = new DocumentTextScanner(Adapter, "{", "}", escapeCharacter: '\\');

		private static readonly ITemplateScanner<Document, DocumentToken<Node>> StrictScanner = new DocumentTextScanner(Adapter, "{", "}", strict: true);

		private static readonly ITemplateScanner<Document, DocumentToken<Node>> DblScanner = new DocumentTextScanner(Adapter, "{{", "}}", strict: true);

		#region Strict Scanning

		[TestMethod]
		public void GetTokens_UnbalancedStartToken_ThrowsErrors()
		{
			var doc = DocumentConverter.FromStrings(new[] { "My favorite colors are { Color1, Color2, and Color3." });

			AssertException
				.OfType<UnbalancedTextTokenException>()
				.WithMessage("Found unbalanced token with text \"{ Color1, Color2, and Color3.\".")
				.IsThrownBy(() => Assert.AreEqual(0, StrictScanner.GetTokens(doc).Count()));
		}

		[TestMethod]
		public void GetTokens_UnbalancedEndToken_ThrowsErrors()
		{
			var doc = DocumentConverter.FromStrings(new[] { "My favorite colors are Color1, Color2 }, and Color3." });

			AssertException
				.OfType<UnbalancedTextTokenException>()
				.WithMessage("Found unbalanced token with text \"My favorite colors are Color1, Color2 }\".")
				.IsThrownBy(() => Assert.AreEqual(0, StrictScanner.GetTokens(doc).Count()));
		}

		[TestMethod]
		public void GetTokens_NestedTokensUnbalanced_ThrowsErrors()
		{
			var doc = DocumentConverter.FromStrings(new[] { "My favorite colors are { Color1, { Color2 }, and Color3." });

			AssertException
				.OfType<UnbalancedTextTokenException>()
				.WithMessage("Found unbalanced token with text \"{ Color1, { Color2 }\".")
				.IsThrownBy(() => Assert.AreEqual(0, StrictScanner.GetTokens(doc).Count()));
		}

		[TestMethod]
		public void GetTokens_NestedTokensMultipleUnbalanced_ThrowsErrors()
		{
			var doc = DocumentConverter.FromStrings(new[] { "My favorite colors are { Color1, { ? } } Color2 }, and Color3." });

			AssertException
				.OfType<UnbalancedTextTokenException>()
				.WithMessage("Found unbalanced token with text \"{ Color1, { ? }\".")
				.IsThrownBy(() => Assert.AreEqual(0, StrictScanner.GetTokens(doc).Count()));
		}

		#endregion

		#region Nested & Unbalanced Token Markers

		[TestMethod]
		public void GetTokens_NestedMarkers_Escaped_BalancedFormatToken()
		{
			TestScan("{ string.Format(\"Favorite Colors: \\{0\\}\", string.Join(\",\", Colors)) }",
				LooseScanner,
				" string.Format(\"Favorite Colors: {0}\", string.Join(\",\", Colors)) "
				);
		}

		[TestMethod]
		public void GetTokens_NestedMarkers_NotEscaped_UnbalancedFormatToken()
		{
			TestScan("{ string.Format(\"Favorite Colors: {0\", string.Join(\",\", Colors)) }",
				LooseScanner,
				"0\", string.Join(\",\", Colors)) "
				);
		}

		[TestMethod]
		public void GetTokens_NestedMarkers_Escaped_UnbalancedFormatToken()
		{
			TestScan("{ string.Format(\"Favorite Colors: \\{0\", string.Join(\",\", Colors)) }",
				LooseScanner,
				" string.Format(\"Favorite Colors: {0\", string.Join(\",\", Colors)) "
				);
		}

		[TestMethod]
		public void GetTokens_NestedMarkers_NotEscaped_BalancedFormatToken()
		{
			TestScan("{ string.Format(\"Favorite Colors: {0}\", string.Join(\",\", Colors)) }",
				LooseScanner,
				"0"
				// TODO: Take into account string literals.
				//" string.Format(\"Favorite Colors: {0}\", string.Join(\",\", Colors)) "
				);
		}

		[TestMethod]
		public void GetTokens_NestedMarkers_NotEscaped_BalancedStringLiteral()
		{
			TestScan("{ \"Options: {\" + string.Join(\",\", Options)) + \"}\" }",
				LooseScanner,
				"\" + string.Join(\",\", Options)) + \""
				// TODO: Take into account string literals.
				//" \"Options: {\" + string.Join(\",\", Options)) + \"}\" "
				);
		}

		[TestMethod]
		public void GetTokens_NestedMarkers_NotEscaped_UnbalancedStringLiteral()
		{
			TestScan("{ \"Items {=\" + string.Join(\",\", Items)) }",
				LooseScanner,
				"=\" + string.Join(\",\", Items)) "
				// TODO: Take into account string literals.
				//" \"Items {=\" + string.Join(\",\", Items)) "
				);
		}

		[TestMethod]
		public void GetTokens_NestedMarkers_Escaped_BalancedStringLiteral()
		{
			TestScan("{ \"Options: \\{\" + string.Join(\",\", Options)) + \"\\}\" }",
				LooseScanner,
				" \"Options: {\" + string.Join(\",\", Options)) + \"}\" "
				);
		}

		[TestMethod]
		public void GetTokens_NestedMarkers_Escaped_UnbalancedStringLiteral()
		{
			TestScan("{ \"Options: \\{\" + string.Join(\",\", Options)) }",
				LooseScanner,
				" \"Options: {\" + string.Join(\",\", Options)) "
				);
		}

		[TestMethod]
		public void GetTokens_NestedMarkers_Unescaped_BalancedAndImmediatelyWrapped()
		{
			TestScan("{{Text}}",
				LooseScanner,
				"Text"
				);
		}

		[TestMethod]
		public void GetTokens_NestedMarkers_Unescaped_BalancedAndImmediatelyWrappedWithWhitespace()
		{
			TestScan("{  {Text} }",
				LooseScanner,
				"Text"
				);
		}

		[TestMethod]
		public void GetTokens_NestedMarkers_Unescaped_BalancedAndWrappedWithOtherText()
		{
			TestScan("{= {Text} }",
				LooseScanner,
				"Text"
				);
		}

		[TestMethod]
		public void GetTokens_NestedMarkers_Unescaped_BalancedAndImmediatelyWrappingMultipleTokens()
		{
			TestScan("Distributivity: {{Var1} + {Var2}, {Var3}} = {{Var1}, {Var3}} + {{Var2}, {Var3}}",
				LooseScanner,
				"Var1",
				"Var2",
				"Var3",
				"Var1",
				"Var3",
				"Var2",
				"Var3"
				);
		}

		[TestMethod]
		public void GetTokens_UnbalancedMarkers_TrailingEndMarker()
		{
			TestScan("{Text}}",
				LooseScanner,
				"Text"
				);
		}

		[TestMethod]
		public void GetTokens_UnbalancedMarkers_TrailingEndMarkerWithText()
		{
			TestScan("{Text} -}",
				LooseScanner,
				"Text"
				);
		}

		[TestMethod]
		public void GetTokens_UnbalancedMarkers_LeadingStartMarker()
		{
			TestScan("{{Text}",
				LooseScanner,
				"Text"
				);
		}

		[TestMethod]
		public void GetTokens_UnbalancedMarkers_LeadingStartMarkerWithText()
		{
			TestScan("{... {Text}",
				LooseScanner,
				"Text"
				);
		}

		#endregion

		#region Multiple Tokens

		[TestMethod]
		public void GetTokens_SingleRunWithMultipleTokens_MultipleTokens()
		{
			TestScan("My favorite colors are { Color1 }, { Color2 }, and { Color3 }.",
				LooseScanner,
				" Color1 ",
				" Color2 ",
				" Color3 "
				);
		}

		#endregion

		#region Run Boundaries

		[TestMethod]
		public void GetTokens_MultipleRunsWithOneTokenEach_MultipleTokens()
		{
			var doc = DocumentConverter.FromStrings(new[] { "My favorite colors are { Color1 },", " { Color2 },", " and { Color3 }." });

			var tokens = LooseScanner.GetTokens(doc).ToArray();

			Assert.AreEqual(" Color1 , Color2 , Color3 ",
				string.Join(",", tokens.Select(t => t.Value)));

			Assert.AreEqual(tokens[0].End, tokens[0].Start);
			Assert.AreEqual("{ Color1 }", ((Run)tokens[0].Start).Text);

			Assert.AreEqual(tokens[1].End, tokens[1].Start);
			Assert.AreEqual("{ Color2 }", ((Run)tokens[1].Start).Text);

			Assert.AreEqual(tokens[2].End, tokens[2].Start);
			Assert.AreEqual("{ Color3 }", ((Run)tokens[2].Start).Text);
		}

		[TestMethod]
		public void GetTokens_SingleTokenWithStartAndEndInDifferentRuns_SingleToken()
		{
			Run[] runs;
			var doc = DocumentConverter.FromStrings(new[] { "My favorite color is { Favorite", "Color }." }, out runs);

			var tokens = LooseScanner.GetTokens(doc).ToArray();

			Assert.AreEqual(" FavoriteColor ",
				string.Join(",", tokens.Select(t => t.Value)));

			Assert.AreEqual(runs[0], tokens[0].Start);
			Assert.AreEqual("{ Favorite", ((Run)tokens[0].Start).Text);
			Assert.AreEqual(runs[1], tokens[0].End);
			Assert.AreEqual("Color }", ((Run)tokens[0].End).Text);
		}

		[TestMethod]
		public void GetTokens_SingleTokenThatSpansSeveralRuns_SingleToken()
		{
			Run[] runs;
			var doc = DocumentConverter.FromStrings(new[] { "My favorite color is { Favorite", "Color", " }." }, out runs);

			var tokens = LooseScanner.GetTokens(doc).ToArray();

			Assert.AreEqual(" FavoriteColor ",
				string.Join(",", tokens.Select(t => t.Value)));

			Assert.AreEqual(runs[0], tokens[0].Start);
			Assert.AreEqual("{ Favorite", ((Run)tokens[0].Start).Text);
			Assert.AreEqual(runs[2], tokens[0].End);
			Assert.AreEqual(" }", ((Run)tokens[0].End).Text);
		}

		[TestMethod]
		public void GetTokens_SingleTokenWhereStartCrossesRunBoundary_SingleToken()
		{
			Run[] runs;
			var doc = DocumentConverter.FromStrings(new[] { "My favorite color is {", "{ FavoriteColor", " }}." }, out runs);

			var tokens = DblScanner.GetTokens(doc).ToArray();

			Assert.AreEqual(" FavoriteColor ",
				string.Join(",", tokens.Select(t => t.Value)));

			Assert.AreEqual(runs[0], tokens[0].Start);
			Assert.AreEqual("{", ((Run)tokens[0].Start).Text);
			Assert.AreEqual(runs[2], tokens[0].End);
			Assert.AreEqual(" }}", ((Run)tokens[0].End).Text);
		}

		[TestMethod]
		public void GetTokens_SingleTokenWhereEndCrossesRunBoundary_SingleToken()
		{
			Run[] runs;
			var doc = DocumentConverter.FromStrings(new[] { "My favorite color is {{ Favorite", "Color }", "}." }, out runs);

			var tokens = DblScanner.GetTokens(doc).ToArray();

			Assert.AreEqual(" FavoriteColor ",
				string.Join(",", tokens.Select(t => t.Value)));

			Assert.AreEqual(runs[0], tokens[0].Start);
			Assert.AreEqual("{{ Favorite", ((Run)tokens[0].Start).Text);
			Assert.AreEqual(runs[2], tokens[0].End);
			Assert.AreEqual("}", ((Run)tokens[0].End).Text);
		}

		[TestMethod]
		public void GetTokens_SingleTokenWhereStartAndEndCrossRunBoundary_SingleToken()
		{
			Run[] runs;
			var doc = DocumentConverter.FromStrings(new[] {  "My favorite color is {", "{ FavoriteColor }", "}." }, out runs);

			var tokens = DblScanner.GetTokens(doc).ToArray();

			Assert.AreEqual(" FavoriteColor ",
				string.Join(",", tokens.Select(t => t.Value)));

			Assert.AreEqual(runs[0], tokens[0].Start);
			Assert.AreEqual("{", ((Run)tokens[0].Start).Text);
			Assert.AreEqual(runs[2], tokens[0].End);
			Assert.AreEqual("}", ((Run)tokens[0].End).Text);
		}

		[TestMethod]
		public void GetTokens_TokenWithSubstringBookmarked_SingleToken()
		{
			Run[] runs;
			var doc = DocumentConverter.FromStrings(new[] { "My favorite color is {", "{ Favor", "@{8728}", "iteCo", "@{/8728}", "lor }", "}." }, out runs);

			var tokens = DblScanner.GetTokens(doc).ToArray();

			Assert.AreEqual(" FavoriteColor ",
				string.Join(",", tokens.Select(t => t.Value)));

			Assert.AreEqual(runs[0], tokens[0].Start);
			Assert.AreEqual("{", ((Run)tokens[0].Start).Text);
			Assert.AreEqual(runs[4], tokens[0].End);
			Assert.AreEqual("}", ((Run)tokens[0].End).Text);
		}

		[TestMethod]
		public void GetTokens_TokenWithSubstringCommented_SingleToken()
		{
			Run[] runs;
			var doc = DocumentConverter.FromStrings(new[] { "My favorite color is {", "{ Favor", "!{5158}", "iteCo", "!{/5158}", "lor }", "}." }, out runs);

			var tokens = DblScanner.GetTokens(doc).ToArray();

			Assert.AreEqual(" FavoriteColor ",
				string.Join(",", tokens.Select(t => t.Value)));

			Assert.AreEqual(runs[0], tokens[0].Start);
			Assert.AreEqual("{", ((Run)tokens[0].Start).Text);
			Assert.AreEqual(runs[4], tokens[0].End);
			Assert.AreEqual("}", ((Run)tokens[0].End).Text);
		}

		#endregion

		#region Hyperlink - URI Encoding

		[TestMethod]
		public void GetTokens_TokenWithinHyperlinkUrl_UrlDecoded()
		{
			var doc = new Document();

			var builder = new DocumentBuilder(doc);

			builder.InsertRun("^");

			builder.InsertField("HYPERLINK \"%7b%7bUrl%7d%7d\"");

			builder.InsertRun("$");

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>^</Run>
				        <FieldStart></FieldStart>
				        <Run>HYPERLINK ""%7b%7bUrl%7d%7d""</Run>
				        <FieldSeparator></FieldSeparator>
				        <FieldEnd></FieldEnd>
				        <Run>$</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");

			var tokens = DblScanner.GetTokens(doc).ToArray();

			Assert.AreEqual("Url",
				string.Join(",", tokens.Select(t => t.Value)));

			Assert.IsInstanceOfType(tokens.Single(), typeof(DocumentToken<Node>));

			Assert.AreEqual(DocumentTextEncoding.None, tokens.Single().Encoding);

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>^</Run>
				        <FieldStart></FieldStart>
				        <Run>HYPERLINK ""</Run>
				        <Run>%7b%7bUrl%7d%7d</Run>
				        <Run>""</Run>
				        <FieldSeparator></FieldSeparator>
				        <FieldEnd></FieldEnd>
				        <Run>$</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");
		}

		[TestMethod]
		public void GetTokens_TokenWithinHyperlinkQueryString_UrlDecoded()
		{
			var doc = new Document();

			var builder = new DocumentBuilder(doc);

			builder.InsertRun("^");

			builder.InsertField("HYPERLINK \"https://google.com/q=%7b%7bQuery%7d%7d\"");

			builder.InsertRun("$");

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>^</Run>
				        <FieldStart></FieldStart>
				        <Run>HYPERLINK ""https://google.com/q=%7b%7bQuery%7d%7d""</Run>
				        <FieldSeparator></FieldSeparator>
				        <FieldEnd></FieldEnd>
				        <Run>$</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");

			var tokens = DblScanner.GetTokens(doc).ToArray();

			Assert.AreEqual("Query",
				string.Join(",", tokens.Select(t => t.Value)));

			Assert.IsInstanceOfType(tokens.Single(), typeof(DocumentToken<Node>));

			Assert.AreEqual(DocumentTextEncoding.Uri, tokens.Single().Encoding);

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>^</Run>
				        <FieldStart></FieldStart>
				        <Run>HYPERLINK ""</Run>
				        <Run>https://google.com/q=</Run>
				        <Run>%7b%7bQuery%7d%7d</Run>
				        <Run>""</Run>
				        <FieldSeparator></FieldSeparator>
				        <FieldEnd></FieldEnd>
				        <Run>$</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");
		}

		#endregion
	}
}
