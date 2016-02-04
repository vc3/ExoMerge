using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace ExoMerge.UnitTests.Html
{
	public static class HtmlFormatter
	{
		private static readonly Regex tagExpr = new Regex("(?<prefix></?)(?<name>[A-Za-z]+)(?<attributes>[^/>]*)(?<suffix>/?>)");

		private static readonly Regex tagBoundaryExpr = new Regex(">\\s*<");

		private static readonly Regex attributeExpr = new Regex("(?<name>[A-Za-z\\-]+)=(?<value>(?<quoted>\"[^\"]*\")|(?<unquoted>[^\"\\s/>]+))");

		private static readonly Regex styleAttributeExpr = new Regex("style=\"(?<value>[^\"]*)\"");

		private static readonly Regex cssNameValueExpr = new Regex("(?<name>[A-Za-z\\-]+):\\s*(?<value>[^;]+)");

		private static readonly string[] directionalCssStyles = new[] { "border", "margin", "padding" };

		internal static string AlphabetizeStyles(string html)
		{
			var result = "";
			var remainder = html;
			while (remainder.Length > 0)
			{
				var styleAttrMatch = styleAttributeExpr.Match(remainder);

				if (!styleAttrMatch.Success)
				{
					result += remainder;
					break;
				}

				result += remainder.Substring(0, styleAttrMatch.Index);

				result += "style=\"" +
						string.Join("; ", styleAttrMatch.Groups["value"].Value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).OrderBy(s => s).ToArray()) +
						"\"";

				remainder = styleAttrMatch.Index + styleAttrMatch.Value.Length < remainder.Length ? remainder.Substring(styleAttrMatch.Index + styleAttrMatch.Value.Length) : "";
			}

			return result;
		}

		internal static string LowerCaseTagNames(string html)
		{
			return tagExpr.Replace(html, tagMatch =>
			{
				var tagPrefix = tagMatch.Groups["prefix"].Value;
				var tagName = tagMatch.Groups["name"].Value;
				return tagMatch.Value.Replace(tagPrefix + tagName, tagPrefix + tagName.ToLower());
			});
		}

		internal static string LowerCaseAttributes(string html)
		{
			return attributeExpr.Replace(html, attrMatch =>
			{
				var attrName = attrMatch.Groups["name"].Value;
				return attrMatch.Value.Replace(attrName, attrName.ToLower());
			});
		}

		internal static string EnsureAttributesQuoted(string html)
		{
			return attributeExpr.Replace(html, attrMatch =>
			{
				var quotedValue = attrMatch.Groups["quoted"].Value;
				var unquotedValue = attrMatch.Groups["unquoted"].Value;
				if (unquotedValue.Length > quotedValue.Length)
					return attrMatch.Value.Replace(unquotedValue, "\"" + unquotedValue + "\"");
				return attrMatch.Value;
			});
		}

		internal static string ExpandDirectionalStyles(string html)
		{
			return styleAttributeExpr.Replace(html, styleAttrMatch =>
			{
				var stylesList = styleAttrMatch.Groups["value"].Value;
				return styleAttrMatch.Value.Replace(stylesList, cssNameValueExpr.Replace(stylesList, nameValueMatch =>
				{
					var styleName = nameValueMatch.Groups["name"].Value.ToLower();
					var styleValue = nameValueMatch.Groups["value"].Value.ToLower();
					if (directionalCssStyles.Contains(styleName))
						return string.Format("{0}-bottom: {1}; {0}-left: {1}; {0}-right: {1}; {0}-top: {1}", styleName, styleValue);
					return nameValueMatch.Value;
				}));
			});
		}

		internal static string CollapseDirectionalStyles(string result)
		{
			throw new NotImplementedException();
		}

		internal static string TagsOnNewLines(string html)
		{
			return tagBoundaryExpr.Replace(html, ">" + Environment.NewLine + "<");
		}

		internal static string LowerCaseCssNames(string html)
		{
			return styleAttributeExpr.Replace(html, styleAttrMatch =>
			{
				var stylesList = styleAttrMatch.Groups["value"].Value;
				return styleAttrMatch.Value.Replace(stylesList, cssNameValueExpr.Replace(stylesList, nameValueMatch =>
				{
					var styleName = nameValueMatch.Groups["name"].Value;
					return nameValueMatch.Value.Replace(styleName, styleName.ToLower());
				}));
			});
		}

		internal static string AlphabetizeAttributes(string html)
		{
			return tagExpr.Replace(html, tagMatch =>
			{
				var tagAttributes = tagMatch.Groups["attributes"].Value;
				if (tagAttributes.Length > 0)
				{
					var tagAttributesList = new List<string>();
					attributeExpr.Replace(tagAttributes, attrMatch =>
					{
						tagAttributesList.Add(attrMatch.Value);
						return "";
					});

					var tagAttributesSorted = string.Join("", tagAttributesList.OrderBy(attr => attr).Select(attr => " " + attr).ToArray());
					return tagMatch.Value.Replace(tagAttributes, tagAttributesSorted);
				}
				return tagMatch.Value;
			});
		}

		internal static string IndentHtml(string html)
		{
			throw new NotImplementedException();
		}
	}
}
