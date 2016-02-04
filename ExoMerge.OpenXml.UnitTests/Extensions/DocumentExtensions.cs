using System;
using System.Linq;
using DocumentFormat.OpenXml.Packaging;
using ExoMerge.UnitTests.Assertions;
using ExoMerge.UnitTests.Extensions;

namespace ExoMerge.OpenXml.UnitTests.Extensions
{
	public static class DocumentExtensions
	{
		public static void AssertText(this WordprocessingDocument document, string text, bool convertFieldCodes = false)
		{
			// Trim leading whitespace from lines and add the paragraph symbol at the end of each line.
			var expectedLines = text.Split(new[] { "\r\n" }, StringSplitOptions.None);
			var expectedText = string.Join("¶\r\n", expectedLines.Select(l => l.TrimStart()));

			var actualText = new DocumentAdapter().GetMarkdown(document);

			// Add the paragraph symbol at the end of each line.
			actualText = actualText.Replace("\r\n", "¶\r\n");

			AssertLines.Match(expectedText, actualText, "¶\r\n");
		}
	}
}
