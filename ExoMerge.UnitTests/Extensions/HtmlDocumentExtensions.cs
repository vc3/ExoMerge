using System;
using ExoMerge.UnitTests.Assertions;
using ExoMerge.UnitTests.Common;
using ExoMerge.UnitTests.Html;
using HtmlAgilityPack;

namespace ExoMerge.UnitTests.Extensions
{
	public static class HtmlDocumentExtensions
	{
		public static bool TryMerge(this HtmlDocument document, object source, out IMergeError[] errors)
		{
			var mergeProvider = new HtmlDocumentMergeProvider<Type, object, string>(new SimpleTokenParser(), "{{", "}}", new SimpleExpressionParser(), null, new SimpleDataProvider());
			return mergeProvider.TryMerge(document, source, MergeErrorAction.Ignore, out errors);
		}

		public static void AssertMatch(this HtmlDocument document, string expected)
		{
			var expectedHtmlLines = expected.FormatHtml();
			var actualHtmlLines = document.DocumentNode.OuterHtml.FormatHtml();
			AssertLines.Match(expectedHtmlLines, actualHtmlLines, "\r\n");
		}
	}
}
