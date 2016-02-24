using System;
using System.Linq;
using Aspose.Words;
using AsposeSerializer;
using AsposeSerializer.Extensions;
using ExoMerge.Analysis;
using ExoMerge.Aspose.UnitTests.Common;
using ExoMerge.Aspose.UnitTests.Extensions;
using ExoMerge.Aspose.UnitTests.Helpers;
using ExoMerge.Documents;
using ExoMerge.Rendering;
using ExoMerge.Structure;
using ExoMerge.UnitTests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.Aspose.UnitTests
{
	[TestClass]
	public class NodeTextDocumentWriterTests : DocumentTestsBase
	{
		private static readonly IDocumentAdapter<Document, Node> Adapter = new DocumentAdapter();

		private static readonly ITemplateScanner<Document, DocumentToken<Node>> Scanner = new DocumentTextScanner<Document, Node>(Adapter, "{{", "}}");

		private static readonly ITokenParser<Type> TokenParser = new SimpleTokenParser();

		private static readonly IExpressionParser<Type, string> ExpressionParser = new SimpleExpressionParser();

		private static readonly IMergeWriter<Document, Node, DocumentToken<Node>> Writer = new DocumentMergeWriter<Document, Node>(Adapter);

		[TestMethod]
		public void RemoveToken_SingleToken_OnlyTextRemains()
		{
			var doc = DocumentConverter.FromStrings(new[]{ "^", "{{ FieldToRemove }}", "$", });

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>^</Run>
				        <Run>{{ FieldToRemove }}</Run>
				        <Run>$</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");

			var tokenToRemove = Scanner.GetTokens(doc).Single();

			Writer.RemoveToken(doc, tokenToRemove);

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>^</Run>
				        <Run>$</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");
		}

		[TestMethod]
		public void RemoveToken_MultipleTokens_SecondTokenRemains()
		{
			var doc = DocumentConverter.FromStrings(new[] { "^", "{{ FieldToRemove }}", "{{ FieldToLeave }}", "$" });

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>^</Run>
				        <Run>{{ FieldToRemove }}</Run>
				        <Run>{{ FieldToLeave }}</Run>
				        <Run>$</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");

			var tokenToRemove = Scanner.GetTokens(doc).First();

			Writer.RemoveToken(doc, tokenToRemove);

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>^</Run>
				        <Run>{{ FieldToLeave }}</Run>
				        <Run>$</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");
		}

		[TestMethod]
		public void ReplaceToken_SingleToken_FirstTokenReplacedAndOuterTextRemains()
		{
			var doc = DocumentConverter.FromStrings(new[] { "^", "{{ FieldToReplace }}", "$" });

			AssertText(doc, doc.ToString(SaveFormat.Text));

			var tokenToReplace = Scanner.GetTokens(doc).First();

			var newRun = Adapter.CreateTextRun(doc, "xyz");

			Writer.ReplaceToken(doc, tokenToReplace, newRun);

			AssertText(doc, "^xyz$\r\n");

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>^</Run>
				        <Run>xyz</Run>
				        <Run>$</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");
		}

		[TestMethod]
		public void ReplaceToken_MultipleTokens_FirstTokenReplacedAndSecondTokenRemains()
		{
			var doc = DocumentConverter.FromStrings(new[] { "^", "{{ FieldToReplace }}", "{{ FieldToLeave }}", "$" });

			AssertText(doc, doc.ToString(SaveFormat.Text));

			var tokenToReplace = Scanner.GetTokens(doc).First();

			var newRun = Adapter.CreateTextRun(doc, "xyz");

			Writer.ReplaceToken(doc, tokenToReplace, newRun);

			AssertText(doc, "^xyz{{ FieldToLeave }}$\r\n");

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>^</Run>
				        <Run>xyz</Run>
				        <Run>{{ FieldToLeave }}</Run>
				        <Run>$</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");
		}

		[TestMethod]
		public void RemoveRegionNodes_AdditionalFieldsAndText_EverythingElseRemains()
		{
			var doc = DocumentConverter.FromStrings(new[] { "^", "{{ PrecedingField }}", "{{ each Items }}", "{{ Sequence }}", "{{ end each }}", "$" });

			var root = MergeTemplateCompiler.Compile<Document, Node, DocumentToken<Node>, Type, object, string>(typeof(object), TokenParser, ExpressionParser, Scanner.GetTokens(doc));
			var repeatableToRemove = root.Children.OfType<IRegion<DocumentToken<Node>>>().OfType<Repeatable<Document, Node, DocumentToken<Node>, Type, object, string>>().Single();

			Writer.RemoveRegionNodes(doc, repeatableToRemove.StartToken, repeatableToRemove.EndToken, RegionNodes.Content);

			AssertText(doc, "^{{ PrecedingField }}{{ each Items }}{{ end each }}$\r\n");

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>^</Run>
				        <Run>{{ PrecedingField }}</Run>
				        <Run>{{ each Items }}</Run>
				        <Run>{{ end each }}</Run>
				        <Run>$</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");
		}

		[TestMethod]
		public void CloneRegion_RepeatableWithNoNestedRegions_NodesAndFieldsAreCloned()
		{
			var doc = new Document();

			var runTexts = new[] { "^", "{{ PrecedingField }}", "{{ each Items }}", "{{ Sequence }}", "{{ end each }}", "$" };

			var importedNodes = doc.Import(doc.Sections.Cast<Section>().Single().Body.Paragraphs.Cast<Paragraph>().Single(),
				DocumentTextFormat.Xml,
				"<Paragraph>\r\n" + string.Join("\r\n", runTexts.Select(t => "<Run>" + t + "</Run>")) + "\r\n</Paragraph>"
				);

			var runs = importedNodes.Cast<Run>().ToArray();

			var root = MergeTemplateCompiler.Compile<Document, Node, DocumentToken<Node>, Type, object, string>(typeof(object), TokenParser, ExpressionParser, Scanner.GetTokens(doc));
			var repeatable = root.Children.OfType<IRegion<DocumentToken<Node>>>().OfType<Repeatable<Document, Node, DocumentToken<Node>, Type, object, string>>().Single();

			Assert.AreEqual(runs[2], repeatable.StartToken.Start);
			Assert.AreEqual(runs[4], repeatable.EndToken.Start);

			var existingInnerTokens = repeatable.GetInnerTokens().ToArray();

			var clonedTokens = Writer.CloneRegion(doc,
				new Tuple<DocumentToken<Node>, DocumentToken<Node>[], DocumentToken<Node>>(repeatable.StartToken, existingInnerTokens, repeatable.EndToken));

			AssertText(doc, "^{{ PrecedingField }}{{ each Items }}{{ Sequence }}{{ end each }}{{ each Items }}{{ Sequence }}{{ end each }}$\r\n");

			Assert.AreNotEqual(repeatable.StartToken, clonedTokens.Item1);
			Assert.AreNotEqual(repeatable.StartToken.Start, clonedTokens.Item1.Start);
			Assert.AreNotEqual(repeatable.StartToken.End, clonedTokens.Item1.End);

			Assert.AreEqual(existingInnerTokens.Length, clonedTokens.Item2.Length);

			Assert.AreNotEqual(repeatable.EndToken, clonedTokens.Item3);
			Assert.AreNotEqual(repeatable.EndToken.Start, clonedTokens.Item3.Start);
			Assert.AreNotEqual(repeatable.EndToken.End, clonedTokens.Item3.End);

			doc.AssertXml(@"
				<Document>
				  <Section>
					<Body>
					  <Paragraph>
					    <Run>^</Run>
					    <Run>{{ PrecedingField }}</Run>
					    <Run>{{ each Items }}</Run>
					    <Run>{{ Sequence }}</Run>
					    <Run>{{ end each }}</Run>
					    <Run>{{ each Items }}</Run>
					    <Run>{{ Sequence }}</Run>
					    <Run>{{ end each }}</Run>
					    <Run>$</Run>
					  </Paragraph>
					</Body>
				  </Section>
				</Document>");
		}

		[TestMethod]
		public void CloneRegion_RepeatableWithNoNestedRegionsOrFields_NodesAreCloned()
		{
			Run[] runs;
			var doc = DocumentConverter.FromStrings(new[] { "^", "{{ PrecedingField }}", "{{ each Items }}", "ITEM", "{{ end each }}", "$", }, out runs);

			AssertText(doc, doc.ToString(SaveFormat.Text));

			var root = MergeTemplateCompiler.Compile<Document, Node, DocumentToken<Node>, Type, object, string>(typeof(object), TokenParser, ExpressionParser, Scanner.GetTokens(doc));
			var repeatable = root.Children.OfType<IRegion<DocumentToken<Node>>>().OfType<Repeatable<Document, Node, DocumentToken<Node>, Type, object, string>>().Single();

			Assert.AreEqual(runs[2], repeatable.StartToken.Start);
			Assert.AreEqual(runs[4], repeatable.EndToken.Start);

			var existingInnerTokens = repeatable.GetInnerTokens().ToArray();

			var clonedTokens = Writer.CloneRegion(doc,
				new Tuple<DocumentToken<Node>, DocumentToken<Node>[], DocumentToken<Node>>(repeatable.StartToken, existingInnerTokens, repeatable.EndToken));

			AssertText(doc, "^{{ PrecedingField }}{{ each Items }}ITEM{{ end each }}{{ each Items }}ITEM{{ end each }}$\r\n");

			Assert.AreNotEqual(repeatable.StartToken, clonedTokens.Item1);
			Assert.AreNotEqual(repeatable.StartToken.Start, clonedTokens.Item1.Start);
			Assert.AreNotEqual(repeatable.StartToken.End, clonedTokens.Item1.End);

			Assert.AreEqual(existingInnerTokens.Length, clonedTokens.Item2.Length);

			Assert.AreNotEqual(repeatable.EndToken, clonedTokens.Item3);
			Assert.AreNotEqual(repeatable.EndToken.Start, clonedTokens.Item3.Start);
			Assert.AreNotEqual(repeatable.EndToken.End, clonedTokens.Item3.End);

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>^</Run>
				        <Run>{{ PrecedingField }}</Run>
				        <Run>{{ each Items }}</Run>
				        <Run>ITEM</Run>
				        <Run>{{ end each }}</Run>
				        <Run>{{ each Items }}</Run>
				        <Run>ITEM</Run>
				        <Run>{{ end each }}</Run>
				        <Run>$</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");
		}

		[TestMethod]
		public void CloneRegion_RepeatableThatSpansParagraphs()
		{
			var doc = new Document();

			var runTexts1 = new[] { "^", "{{ PrecedingField }}", "{{ each Items }}", };

			var onlySection = doc.Sections.Cast<Section>().Single();

			var onlyPara = onlySection.Body.Paragraphs.Cast<Paragraph>().Single();

			onlyPara.ParagraphFormat.SpaceBefore = 10;
			onlyPara.ParagraphFormat.SpaceAfter = 1;

			doc.Import(onlyPara, DocumentTextFormat.Xml, "<Paragraph>\r\n" + string.Join("\r\n", runTexts1.Select(t => "<Run>" + t + "</Run>")) + "\r\n</Paragraph>");

			var runs1 = onlyPara.ChildNodes.Cast<Run>().ToArray();

			var newPara = new Paragraph(doc);

			newPara.ParagraphFormat.SpaceBefore = 1;
			newPara.ParagraphFormat.SpaceAfter = 10;

			onlySection.Body.ChildNodes.Add(newPara);

			var runTexts2 = new[] { "{{ Sequence }}", "{{ end each }}", "$", };

			doc.Import(newPara, DocumentTextFormat.Xml, "<Paragraph>\r\n" + string.Join("\r\n", runTexts2.Select(t => "<Run>" + t + "</Run>")) + "\r\n</Paragraph>");

			var runs2 = newPara.ChildNodes.Cast<Run>().ToArray();

			Container<Document, Node, DocumentToken<Node>, Type, object, string> root;
			MergeTemplateError<DocumentToken<Node>>[] errors;
			MergeTemplateCompiler.Compile(typeof(object), TokenParser, ExpressionParser, Scanner.GetTokens(doc), null, true, out root, out errors);

			var repeatable = root.Children.OfType<IRegion<DocumentToken<Node>>>().OfType<Repeatable<Document, Node, DocumentToken<Node>, Type, object, string>>().Single();

			Assert.AreEqual(runs1[2], repeatable.StartToken.Start);

			Assert.AreEqual(runs2[1], repeatable.EndToken.Start);

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph paragraphformat_spacebefore='10' paragraphformat_spaceafter='1'>
				        <Run>^</Run>
				        <Run>{{ PrecedingField }}</Run>
				        <Run>{{ each Items }}</Run>
				      </Paragraph>
				      <Paragraph paragraphformat_spacebefore='1' paragraphformat_spaceafter='10'>
				        <Run>{{ Sequence }}</Run>
				        <Run>{{ end each }}</Run>
				        <Run>$</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>");

			var existingInnerTokens = repeatable.GetInnerTokens().ToArray();

			var clonedTokens = Writer.CloneRegion(doc,
				new Tuple<DocumentToken<Node>, DocumentToken<Node>[], DocumentToken<Node>>(repeatable.StartToken, existingInnerTokens, repeatable.EndToken));

			AssertText(doc, @"^{{ PrecedingField }}{{ each Items }}
								{{ Sequence }}{{ end each }}
								{{ each Items }}
								{{ Sequence }}{{ end each }}$
								");

			Assert.AreNotEqual(repeatable.StartToken, clonedTokens.Item1);
			Assert.AreNotEqual(repeatable.StartToken.Start, clonedTokens.Item1.Start);
			Assert.AreNotEqual(repeatable.StartToken.End, clonedTokens.Item1.End);

			Assert.AreEqual(existingInnerTokens.Length, clonedTokens.Item2.Length);

			Assert.AreNotEqual(repeatable.EndToken, clonedTokens.Item3);
			Assert.AreNotEqual(repeatable.EndToken.Start, clonedTokens.Item3.Start);
			Assert.AreNotEqual(repeatable.EndToken.End, clonedTokens.Item3.End);

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph paragraphformat_spacebefore='10' paragraphformat_spaceafter='1'>
				        <Run>^</Run>
				        <Run>{{ PrecedingField }}</Run>
				        <Run>{{ each Items }}</Run>
				      </Paragraph>
				      <Paragraph paragraphformat_spacebefore='1' paragraphformat_spaceafter='10'>
				        <Run>{{ Sequence }}</Run>
				        <Run>{{ end each }}</Run>
				      </Paragraph>
				      <Paragraph paragraphformat_spacebefore='10' paragraphformat_spaceafter='1'>
				        <Run>{{ each Items }}</Run>
				      </Paragraph>
				      <Paragraph paragraphformat_spacebefore='1' paragraphformat_spaceafter='10'>
				        <Run>{{ Sequence }}</Run>
				        <Run>{{ end each }}</Run>
				        <Run>$</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>");
		}

		[TestMethod]
		public void CloneRegion_RepeatableThatSpansSections()
		{
			var doc = new Document();

			var runTexts1 = new[] { "^", "{{ PrecedingField }}", "{{ each Items }}", };

			var onlyPara = doc.Sections.Cast<Section>().Single().Body.Paragraphs.Cast<Paragraph>().Single();

			doc.Import(onlyPara, DocumentTextFormat.Xml, "<Paragraph>\r\n" + string.Join("\r\n", runTexts1.Select(t => "<Run>" + t + "</Run>")) + "\r\n</Paragraph>");

			var runs1 = onlyPara.ChildNodes.Cast<Run>().ToArray();

			var newSection = new Section(doc);

			newSection.PageSetup.SectionStart = SectionStart.Continuous;

			doc.Sections.Add(newSection);

			var runTexts2 = new[] { "{{ Sequence }}", "{{ end each }}", "$", };

			doc.Import(newSection, DocumentTextFormat.Xml, "<Section><Body><Paragraph>\r\n" + string.Join("\r\n", runTexts2.Select(t => "<Run>" + t + "</Run>")) + "\r\n</Paragraph></Body></Section>");

			var runs2 = newSection.Body.ChildNodes.Cast<Paragraph>().Single().ChildNodes.Cast<Run>().ToArray();

			Container<Document, Node, DocumentToken<Node>, Type, object, string> root;
			MergeTemplateError<DocumentToken<Node>>[] errors;
			MergeTemplateCompiler.Compile(typeof(object), TokenParser, ExpressionParser, Scanner.GetTokens(doc), null, true, out root, out errors);

			var repeatable = root.Children.OfType<IRegion<DocumentToken<Node>>>().OfType<Repeatable<Document, Node, DocumentToken<Node>, Type, object, string>>().Single();

			Assert.AreEqual(runs1[2], repeatable.StartToken.Start);

			Assert.AreEqual(runs2[1], repeatable.EndToken.Start);

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>^</Run>
				        <Run>{{ PrecedingField }}</Run>
				        <Run>{{ each Items }}</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				  <Section pagesetup_sectionstart='Continuous' pagesetup_headerdistance='36' pagesetup_footerdistance='36'>
				    <Body>
				      <Paragraph>
				        <Run>{{ Sequence }}</Run>
				        <Run>{{ end each }}</Run>
				        <Run>$</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>");

			var existingInnerTokens = repeatable.GetInnerTokens().ToArray();

			var clonedTokens = Writer.CloneRegion(doc,
				new Tuple<DocumentToken<Node>, DocumentToken<Node>[], DocumentToken<Node>>(repeatable.StartToken, existingInnerTokens, repeatable.EndToken));

			AssertText(doc, @"^{{ PrecedingField }}{{ each Items }}
								{{ Sequence }}{{ end each }}
								{{ each Items }}
								{{ Sequence }}{{ end each }}$
								");

			Assert.AreNotEqual(repeatable.StartToken, clonedTokens.Item1);
			Assert.AreNotEqual(repeatable.StartToken.Start, clonedTokens.Item1.Start);
			Assert.AreNotEqual(repeatable.StartToken.End, clonedTokens.Item1.End);

			Assert.AreEqual(existingInnerTokens.Length, clonedTokens.Item2.Length);

			Assert.AreNotEqual(repeatable.EndToken, clonedTokens.Item3);
			Assert.AreNotEqual(repeatable.EndToken.Start, clonedTokens.Item3.Start);
			Assert.AreNotEqual(repeatable.EndToken.End, clonedTokens.Item3.End);

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>^</Run>
				        <Run>{{ PrecedingField }}</Run>
				        <Run>{{ each Items }}</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				  <Section pagesetup_sectionstart='Continuous' pagesetup_headerdistance='36' pagesetup_footerdistance='36'>
				    <Body>
				      <Paragraph>
				        <Run>{{ Sequence }}</Run>
				        <Run>{{ end each }}</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				  <Section pagesetup_sectionstart='Continuous'>
				    <Body>
				      <Paragraph>
				        <Run>{{ each Items }}</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				  <Section pagesetup_sectionstart='Continuous' pagesetup_headerdistance='36' pagesetup_footerdistance='36'>
				    <Body>
				      <Paragraph>
				        <Run>{{ Sequence }}</Run>
				        <Run>{{ end each }}</Run>
				        <Run>$</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>");
		}

		[TestMethod]
		public void CloneRegion_ConditionalWithoutNesting_NodesAndFieldsAreCloned()
		{
			Run[] runs;
			var doc = DocumentConverter.FromStrings(new[] { "^", "{{ PrecedingField }}", "{{ each Items }}", "{{ Sequence }}", "{{ end each }}", "$" }, out runs);

			var root = MergeTemplateCompiler.Compile<Document, Node, DocumentToken<Node>, Type, object, string>(typeof(object), TokenParser, ExpressionParser, Scanner.GetTokens(doc));
			var repeatable = root.Children.OfType<IRegion<DocumentToken<Node>>>().OfType<Repeatable<Document, Node, DocumentToken<Node>, Type, object, string>>().Single();

			Assert.AreEqual(runs[2], repeatable.StartToken.Start);
			Assert.AreEqual(runs[4], repeatable.EndToken.Start);

			var existingInnerTokens = repeatable.GetInnerTokens().ToArray();

			var clonedTokens = Writer.CloneRegion(doc,
				new Tuple<DocumentToken<Node>, DocumentToken<Node>[], DocumentToken<Node>>(repeatable.StartToken, existingInnerTokens, repeatable.EndToken));

			AssertText(doc, "^{{ PrecedingField }}{{ each Items }}{{ Sequence }}{{ end each }}{{ each Items }}{{ Sequence }}{{ end each }}$\r\n");

			Assert.AreNotEqual(repeatable.StartToken, clonedTokens.Item1);
			Assert.AreNotEqual(repeatable.StartToken.Start, clonedTokens.Item1.Start);
			Assert.AreNotEqual(repeatable.StartToken.End, clonedTokens.Item1.End);

			Assert.AreEqual(existingInnerTokens.Length, clonedTokens.Item2.Length);

			Assert.AreNotEqual(repeatable.EndToken, clonedTokens.Item3);
			Assert.AreNotEqual(repeatable.EndToken.Start, clonedTokens.Item3.Start);
			Assert.AreNotEqual(repeatable.EndToken.End, clonedTokens.Item3.End);

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>^</Run>
				        <Run>{{ PrecedingField }}</Run>
				        <Run>{{ each Items }}</Run>
				        <Run>{{ Sequence }}</Run>
				        <Run>{{ end each }}</Run>
				        <Run>{{ each Items }}</Run>
				        <Run>{{ Sequence }}</Run>
				        <Run>{{ end each }}</Run>
				        <Run>$</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");
		}
	}
}
