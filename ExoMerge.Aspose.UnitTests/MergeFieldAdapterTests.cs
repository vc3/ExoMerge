using System;
using System.Linq;
using Aspose.Words;
using Aspose.Words.Fields;
using ExoMerge.Analysis;
using ExoMerge.Aspose.MergeFields;
using ExoMerge.Aspose.UnitTests.Common;
using ExoMerge.Aspose.UnitTests.Helpers;
using ExoMerge.Documents;
using ExoMerge.Rendering;
using ExoMerge.Structure;
using ExoMerge.UnitTests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.Aspose.UnitTests
{
	[TestClass]
	public class MergeFieldAdapterTests : DocumentTestsBase
	{
		private static readonly IDocumentAdapter<Document, Node> Adapter = new DocumentAdapter();

		private static readonly ITemplateScanner<Document, DocumentToken<Node>> Scanner = new MergeFieldScanner();

		private static readonly ITokenParser<Type> TokenParser = new MergeFieldPrefixParser<Type>();

		private static readonly IExpressionParser<Type, string> ExpressionParser = new SimpleExpressionParser();

		private static readonly IMergeWriter<Document, Node, DocumentToken<Node>> Writer = new DocumentMergeWriter<Document, Node>(Adapter);

		[TestMethod]
		public void RemoveToken_SingleToken_OnlyTextRemains()
		{
			var doc = DocumentConverter.FromDisplayCode(@"^{ MERGEFIELD FieldToRemove }$");

			var tokenToRemove = Scanner.GetTokens(doc).Single();

			Writer.RemoveToken(doc, tokenToRemove);

			AssertText(doc, @"^$
							");
		}

		[TestMethod]
		public void RemoveToken_MultipleTokens_SecondTokenRemains()
		{
			var doc = DocumentConverter.FromDisplayCode("^{ MERGEFIELD FieldToRemove }{ MERGEFIELD FieldToLeave }$");

			var tokenToRemove = Scanner.GetTokens(doc).First();

			Writer.RemoveToken(doc, tokenToRemove);

			Assert.AreEqual("^«FieldToLeave»$\r\n", doc.ToString(SaveFormat.Text));
		}

		[TestMethod]
		public void ReplaceToken_SingleToken_FirstTokenReplacedAndOuterTextRemains()
		{
			var doc = DocumentConverter.FromDisplayCode("^{ MERGEFIELD FieldToReplace }$");

			var tokenToReplace = Scanner.GetTokens(doc).First();

			var newRun = Adapter.CreateTextRun(doc, "xyz");

			Writer.ReplaceToken(doc, tokenToReplace, newRun);

			AssertText(doc, "^xyz$\r\n");
		}

		[TestMethod]
		public void ReplaceToken_MultipleTokens_FirstTokenReplacedAndSecondTokenRemains()
		{
			var doc = DocumentConverter.FromDisplayCode("^{ MERGEFIELD FieldToReplace }{ MERGEFIELD FieldToLeave }$");

			var tokenToReplace = Scanner.GetTokens(doc).First();

			var newRun = Adapter.CreateTextRun(doc, "xyz");

			Writer.ReplaceToken(doc, tokenToReplace, newRun);

			AssertText(doc, "^xyz«FieldToLeave»$\r\n");
		}

		[TestMethod]
		public void RemoveRegionNodes_AdditionalFieldsAndText_EverythingElseRemains()
		{
			var doc = DocumentConverter.FromDisplayCode("^{ MERGEFIELD PrecedingField }{ MERGEFIELD TableStart:Items }{ MERGEFIELD Sequence }{ MERGEFIELD TableEnd:Items }$");

			var root = MergeTemplateCompiler.Compile<Document, Node, DocumentToken<Node>, Type, object, string>(typeof(object), TokenParser, ExpressionParser, Scanner.GetTokens(doc));
			var repeatableToRemove = root.Children.OfType<IRegion<DocumentToken<Node>>>().OfType<Repeatable<Document, Node, DocumentToken<Node>, Type, object, string>>().Single();

			Writer.RemoveRegionNodes(doc, repeatableToRemove.StartToken, repeatableToRemove.EndToken, RegionNodes.Content);

			Assert.AreEqual("^«PrecedingField»«TableStart:Items»«TableEnd:Items»$\r\n", doc.ToString(SaveFormat.Text));
		}

		[TestMethod]
		public void CloneRegion_RepeatableWithNoNewstedRegionsOrFields_NodesAreCloned()
		{
			Field[] fields;
			var doc = DocumentConverter.FromDisplayCode("^{ MERGEFIELD PrecedingField }{ MERGEFIELD TableStart:Items }ITEM{ MERGEFIELD TableEnd:Items }$", out fields);

			var root = MergeTemplateCompiler.Compile<Document, Node, DocumentToken<Node>, Type, object, string>(typeof(object), TokenParser, ExpressionParser, Scanner.GetTokens(doc));
			var repeatable = root.Children.OfType<IRegion<DocumentToken<Node>>>().OfType<Repeatable<Document, Node, DocumentToken<Node>, Type, object, string>>().Single();

			Assert.AreEqual(fields[1].Start, repeatable.StartToken.Start);
			Assert.AreEqual(fields[2].Start, repeatable.EndToken.Start);

			var existingInnerTokens = repeatable.GetInnerTokens().ToArray();

			var clonedTokens = Writer.CloneRegion(doc,
				new Tuple<DocumentToken<Node>, DocumentToken<Node>[], DocumentToken<Node>>(repeatable.StartToken, existingInnerTokens, repeatable.EndToken));

			AssertText(doc, @"^«PrecedingField»«TableStart:Items»ITEM«TableEnd:Items»«TableStart:Items»ITEM«TableEnd:Items»$
			                  ");

			Assert.AreNotEqual(repeatable.StartToken, clonedTokens.Item1);
			Assert.AreNotEqual(repeatable.StartToken.Start, clonedTokens.Item1.Start);
			Assert.AreNotEqual(repeatable.StartToken.End, clonedTokens.Item1.End);

			Assert.AreEqual(existingInnerTokens.Length, clonedTokens.Item2.Length);

			Assert.AreNotEqual(repeatable.EndToken, clonedTokens.Item3);
			Assert.AreNotEqual(repeatable.EndToken.Start, clonedTokens.Item3.Start);
			Assert.AreNotEqual(repeatable.EndToken.End, clonedTokens.Item3.End);
		}

		[TestMethod]
		public void CloneRegion_RepeatableWithNoNestedRegions_NodesAndFieldsAreCloned()
		{
			Field[] fields;

			var doc = DocumentConverter.FromDisplayCode(
				@"^{ MERGEFIELD PrecedingField }{ MERGEFIELD TableStart:Items }{ MERGEFIELD Sequence }{ MERGEFIELD TableEnd:Items }$",
				out fields);

			var root = MergeTemplateCompiler.Compile<Document, Node, DocumentToken<Node>, Type, object, string>(typeof(object), TokenParser, ExpressionParser, Scanner.GetTokens(doc));
			var repeatable = root.Children.OfType<IRegion<DocumentToken<Node>>>().OfType<Repeatable<Document, Node, DocumentToken<Node>, Type, object, string>>().Single();

			Assert.AreEqual(fields[1].Start, repeatable.StartToken.Start);
			Assert.AreEqual(fields[3].Start, repeatable.EndToken.Start);

			var existingInnerTokens = repeatable.GetInnerTokens().ToArray();

			var clonedTokens = Writer.CloneRegion(doc,
				new Tuple<DocumentToken<Node>, DocumentToken<Node>[], DocumentToken<Node>>(repeatable.StartToken, existingInnerTokens, repeatable.EndToken));

			AssertText(doc, @"^«PrecedingField»«TableStart:Items»«Sequence»«TableEnd:Items»«TableStart:Items»«Sequence»«TableEnd:Items»$
			                  ");

			Assert.AreNotEqual(repeatable.StartToken, clonedTokens.Item1);
			Assert.AreNotEqual(repeatable.StartToken.Start, clonedTokens.Item1.Start);
			Assert.AreNotEqual(repeatable.StartToken.End, clonedTokens.Item1.End);

			Assert.AreEqual(existingInnerTokens.Length, clonedTokens.Item2.Length);

			Assert.AreNotEqual(repeatable.EndToken, clonedTokens.Item3);
			Assert.AreNotEqual(repeatable.EndToken.Start, clonedTokens.Item3.Start);
			Assert.AreNotEqual(repeatable.EndToken.End, clonedTokens.Item3.End);
		}

		[TestMethod]
		public void CloneRegion_RepeatableThatSpansParagraphs_NodesAndFieldsAreCloned()
		{
			Field[] fields;

			var doc = DocumentConverter.FromDisplayCode(
				@"^
				{ MERGEFIELD PrecedingField }
				{ MERGEFIELD TableStart:Items }
				{ MERGEFIELD Sequence }
				{ MERGEFIELD TableEnd:Items }
				$
				",
				 out fields);

			var root = MergeTemplateCompiler.Compile<Document, Node, DocumentToken<Node>, Type, object, string>(typeof(object), TokenParser, ExpressionParser, Scanner.GetTokens(doc));
			var repeatable = root.Children.OfType<IRegion<DocumentToken<Node>>>().OfType<Repeatable<Document, Node, DocumentToken<Node>, Type, object, string>>().Single();

			Assert.AreEqual(fields[1].Start, repeatable.StartToken.Start);
			Assert.AreEqual(fields[3].Start, repeatable.EndToken.Start);

			var existingInnerTokens = repeatable.GetInnerTokens().ToArray();

			var clonedTokens = Writer.CloneRegion(doc,
				new Tuple<DocumentToken<Node>, DocumentToken<Node>[], DocumentToken<Node>>(repeatable.StartToken, existingInnerTokens, repeatable.EndToken));

			AssertText(doc, @"^
								«PrecedingField»
								«TableStart:Items»
								«Sequence»
								«TableEnd:Items»
								«TableStart:Items»
								«Sequence»
								«TableEnd:Items»
								$
								");

			Assert.AreNotEqual(repeatable.StartToken, clonedTokens.Item1);
			Assert.AreNotEqual(repeatable.StartToken.Start, clonedTokens.Item1.Start);
			Assert.AreNotEqual(repeatable.StartToken.End, clonedTokens.Item1.End);

			Assert.AreEqual(existingInnerTokens.Length, clonedTokens.Item2.Length);

			Assert.AreNotEqual(repeatable.EndToken, clonedTokens.Item3);
			Assert.AreNotEqual(repeatable.EndToken.Start, clonedTokens.Item3.Start);
			Assert.AreNotEqual(repeatable.EndToken.End, clonedTokens.Item3.End);
		}

		[TestMethod]
		public void CloneRegion_RepeatableThatSpansParagraphsAbnormal_NodesAndFieldsAreCloned()
		{
			Field[] fields;

			var doc = DocumentConverter.FromDisplayCode(
				@"^
				{ MERGEFIELD PrecedingField }{ MERGEFIELD TableStart:Items }
				{ MERGEFIELD Sequence }{ MERGEFIELD TableEnd:Items }$
				",
				 out fields);

			var root = MergeTemplateCompiler.Compile<Document, Node, DocumentToken<Node>, Type, object, string>(typeof(object), TokenParser, ExpressionParser, Scanner.GetTokens(doc));
			var repeatable = root.Children.OfType<IRegion<DocumentToken<Node>>>().OfType<Repeatable<Document, Node, DocumentToken<Node>, Type, object, string>>().Single();

			Assert.AreEqual(fields[1].Start, repeatable.StartToken.Start);
			Assert.AreEqual(fields[3].Start, repeatable.EndToken.Start);

			var existingInnerTokens = repeatable.GetInnerTokens().ToArray();

			var clonedTokens = Writer.CloneRegion(doc,
				new Tuple<DocumentToken<Node>, DocumentToken<Node>[], DocumentToken<Node>>(repeatable.StartToken, existingInnerTokens, repeatable.EndToken));

			AssertText(doc, @"^
							«PrecedingField»«TableStart:Items»
							«Sequence»«TableEnd:Items»
							«TableStart:Items»
							«Sequence»«TableEnd:Items»$
							");

			Assert.AreNotEqual(repeatable.StartToken, clonedTokens.Item1);
			Assert.AreNotEqual(repeatable.StartToken.Start, clonedTokens.Item1.Start);
			Assert.AreNotEqual(repeatable.StartToken.End, clonedTokens.Item1.End);

			Assert.AreEqual(existingInnerTokens.Length, clonedTokens.Item2.Length);

			Assert.AreNotEqual(repeatable.EndToken, clonedTokens.Item3);
			Assert.AreNotEqual(repeatable.EndToken.Start, clonedTokens.Item3.Start);
			Assert.AreNotEqual(repeatable.EndToken.End, clonedTokens.Item3.End);
		}
	}
}
