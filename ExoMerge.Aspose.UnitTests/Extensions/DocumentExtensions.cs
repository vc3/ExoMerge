using System;
using System.Linq;
using Aspose.Words;
using AsposeSerializer;
using AsposeSerializer.Extensions;
using ExoMerge.Analysis;
using ExoMerge.Aspose.MergeFields;
using ExoMerge.Aspose.UnitTests.Common;
using ExoMerge.Aspose.UnitTests.Helpers;
using ExoMerge.Rendering;
using ExoMerge.UnitTests.Assertions;
using ExoMerge.UnitTests.Common;
using ExoMerge.UnitTests.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.Aspose.UnitTests.Extensions
{
	public static class DocumentExtensions
	{
		private static readonly IGeneratorFactory<IGenerator<Document, Node, Type, object, string>> NullGeneratorFactory = null;

		private static readonly string expressionParser = "^\\s*[A-Za-z_]+(\\.[A-Za-z_]+)*\\s*$";

		public static bool TryMerge(this Document document, object data, out IMergeError[] errors, bool useMergeFields = false, bool keepEmptyRows = false, MergeErrorAction errorAction = MergeErrorAction.Ignore, IGeneratorFactory<IGenerator<Document, Node, Type, object, string>> generatorFactory = null)
		{
			if (useMergeFields)
			{
				var mergeProvider = new MergeFieldDocumentMergeProvider<Type, object>(
					new MergeFieldPrefixParser<Type>("List", "If", MergeFieldPrefixParser<Type>.RegionNameFormat.EndPrefixOnly),
					new SimpleExpressionParser(expressionParser),
					generatorFactory ?? NullGeneratorFactory,
					new SimpleDataProvider()
					)
				{
					RemoveParentParagraphOfEmptyFields = true,
					KeepEmptyRegionRows = keepEmptyRows,
				};

				return mergeProvider.TryMerge(document, data, errorAction, out errors);
			}
			else
			{
				var mergeProvider = new DocumentTextMergeProvider<Type, object, string>(
					"{{",
					"}}",
					new SimpleTokenParser(),
					new SimpleExpressionParser(expressionParser),
					generatorFactory ?? NullGeneratorFactory,
					new SimpleDataProvider()
					)
				{
					RemoveParentParagraphOfEmptyFields = true,
					KeepEmptyRegionRows = keepEmptyRows,
				};

				return mergeProvider.TryMerge(document, data, errorAction, out errors);
			}
		}

		public static void Merge(this Document document, object data, bool useMergeFields = false, bool keepEmptyRows = false, IGeneratorFactory<IGenerator<Document, Node, Type, object, string>> generatorFactory = null)
		{
			if (useMergeFields)
			{
				var mergeProvider = new MergeFieldDocumentMergeProvider<Type, object>(
					new MergeFieldPrefixParser<Type>("List", "If", MergeFieldPrefixParser<Type>.RegionNameFormat.EndPrefixOnly),
					new SimpleExpressionParser(expressionParser),
					generatorFactory ?? NullGeneratorFactory,
					new SimpleDataProvider()
					)
				{
					RemoveParentParagraphOfEmptyFields = true,
					KeepEmptyRegionRows = keepEmptyRows,
				};

				mergeProvider.Merge(document, data);
			}
			else
			{
				var mergeProvider = new DocumentTextMergeProvider<Type, object, string>(
					"{{",
					"}}",
					new SimpleTokenParser(),
					new SimpleExpressionParser(expressionParser),
					generatorFactory ?? NullGeneratorFactory,
					new SimpleDataProvider()
					)
				{
					RemoveParentParagraphOfEmptyFields = true,
					KeepEmptyRegionRows = keepEmptyRows,
				};

				mergeProvider.Merge(document, data);
			}
		}

		public static void AssertText(this Document document, TestContext testContext, string text, bool hasSaved = false, bool convertFieldCodes = false, bool asMarkdown = false)
		{
			const string noLicenseMessage = @"Evaluation Only. Created with Aspose.Words. Copyright 2003-2011 Aspose Pty Ltd.";

			string expectedText;

			if (asMarkdown)
			{
				var writer = new DocumentTemplateWriter();

				writer.InsertMarkdown(text);

				expectedText = convertFieldCodes
					? DocumentConverter.ConvertCodeToDisplay(writer.Document.GetText().Replace("\r\f", "\r\n").Replace("\f", "\r\n"))
					: string.Join("\r\n", writer.Document.ToString(SaveFormat.Text).Split(new[] { "\r\n" }, StringSplitOptions.None).Select(l => l.TrimStart()));

				expectedText = expectedText.Replace("\r\n", "¶\r\n");
			}
			else
			{
				// Trim leading whitespace from lines and add the paragraph symbol at the end of each line.
				var expectedLines = text.Split(new[] {"\r\n"}, StringSplitOptions.None);
				if (hasSaved && !DocumentTestsBase.HasLicense)
					expectedLines = new[] {noLicenseMessage}.Concat(expectedLines).ToArray();
				expectedText = string.Join("¶\r\n", expectedLines.Select(l => l.TrimStart()));
			}

			var actualText = convertFieldCodes
				? DocumentConverter.ConvertCodeToDisplay(document.GetText().Replace("\r\f", "\r\n").Replace("\f", "\r\n"))
				: string.Join("\r\n", document.ToString(SaveFormat.Text).Split(new[] {"\r\n"}, StringSplitOptions.None).Select(l => l.TrimStart()));

			// Add the paragraph symbol at the end of each line.
			actualText = actualText.Replace("\r\n", "¶\r\n");

			try
			{
				AssertLines.Match(expectedText, actualText, "¶\r\n");
			}
			catch
			{
				document.Save("C:\\Temp\\" + testContext.TestName + ".docx");

				throw;
			}
		}

		public static void AssertXml(this Document document, string expectedXml)
		{
			AssertXml(document, null, expectedXml);
		}

		public static void AssertXml(this Document document, CompositeNode target, string expectedXml)
		{
			var actualXml = document.Export(target, DocumentTextFormat.Xml);

			var expected = string.Join("\r\n", expectedXml.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).Where(s => s.Length > 0));

			var actual = string.Join("\r\n", actualXml.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).Where(s => s.Length > 0));

			AssertLines.Match(expected, actual, "\r\n");
		}
	}
}
