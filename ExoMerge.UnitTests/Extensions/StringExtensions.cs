using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExoMerge.UnitTests.Html;

namespace ExoMerge.UnitTests.Extensions
{
	public static class StringExtensions
	{
		[Flags]
		private enum HtmlWhitespaceOptions
		{
			NewLine = 2,
			Indent = 4
		}

		[Flags]
		private enum HtmlTagOptions
		{
			LowerCase = 2
		}

		[Flags]
		private enum HtmlAttributeOptions
		{
			LowerCase = 2,
			Alphabetized = 4,
			EnsureQuotes = 8
		}

		[Flags]
		private enum HtmlStylesOptions
		{
			LowerCase = 2,
			Alphabetized = 4,
			ExpandDirectionalStyles = 8,
			CollapseDirectionalStyles = 16
		}

		private static string FormatHtml(string html, HtmlWhitespaceOptions whitespace, HtmlTagOptions tags, HtmlAttributeOptions attributes, HtmlStylesOptions styles)
		{
			var result = html;

			if ((whitespace & HtmlWhitespaceOptions.NewLine) == HtmlWhitespaceOptions.NewLine)
				result = HtmlFormatter.TagsOnNewLines(result);

			if ((whitespace & HtmlWhitespaceOptions.Indent) == HtmlWhitespaceOptions.Indent)
				result = HtmlFormatter.IndentHtml(result);

			if ((tags & HtmlTagOptions.LowerCase) == HtmlTagOptions.LowerCase)
				result = HtmlFormatter.LowerCaseTagNames(result);

			if ((attributes & HtmlAttributeOptions.LowerCase) == HtmlAttributeOptions.LowerCase)
				result = HtmlFormatter.LowerCaseAttributes(result);

			if ((attributes & HtmlAttributeOptions.Alphabetized) == HtmlAttributeOptions.Alphabetized)
				result = HtmlFormatter.AlphabetizeAttributes(result);

			if ((attributes & HtmlAttributeOptions.EnsureQuotes) == HtmlAttributeOptions.EnsureQuotes)
				result = HtmlFormatter.EnsureAttributesQuoted(result);

			if ((styles & HtmlStylesOptions.LowerCase) == HtmlStylesOptions.LowerCase)
				result = HtmlFormatter.LowerCaseCssNames(result);

			if ((styles & HtmlStylesOptions.Alphabetized) == HtmlStylesOptions.Alphabetized)
				result = HtmlFormatter.AlphabetizeStyles(result);

			const HtmlStylesOptions expandCollapseDirectionalStyles = HtmlStylesOptions.ExpandDirectionalStyles | HtmlStylesOptions.CollapseDirectionalStyles;
			if ((styles & expandCollapseDirectionalStyles) == expandCollapseDirectionalStyles)
				throw new Exception("Style options \"ExpandDirectionalStyles\" and \"CollapseDirectionalStyles\" are mutually exclusive.");

			if ((styles & HtmlStylesOptions.ExpandDirectionalStyles) == HtmlStylesOptions.ExpandDirectionalStyles)
				result = HtmlFormatter.ExpandDirectionalStyles(result);

			if ((styles & HtmlStylesOptions.CollapseDirectionalStyles) == HtmlStylesOptions.CollapseDirectionalStyles)
				result = HtmlFormatter.CollapseDirectionalStyles(result);

			return result.Trim();
		}

		/// <summary>
		/// Converts the given HTML string into a more readable format.
		/// </summary>
		/// <param name="html">The unformatted source HTML string</param>
		/// <returns>A pretty-printed HTML string</returns>
		internal static string FormatHtml(this string html)
		{
			return FormatHtml(html,
			            HtmlWhitespaceOptions.NewLine,
			            HtmlTagOptions.LowerCase,
						HtmlAttributeOptions.LowerCase | HtmlAttributeOptions.Alphabetized | HtmlAttributeOptions.EnsureQuotes,
			            HtmlStylesOptions.LowerCase | HtmlStylesOptions.Alphabetized | HtmlStylesOptions.ExpandDirectionalStyles);
		}
	}
}
