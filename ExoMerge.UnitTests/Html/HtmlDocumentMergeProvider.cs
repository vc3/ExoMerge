using ExoMerge.Analysis;
using ExoMerge.DataAccess;
using ExoMerge.Documents;
using ExoMerge.Rendering;
using HtmlAgilityPack;

namespace ExoMerge.UnitTests.Html
{
	public class HtmlDocumentMergeProvider<TSourceType, TSource, TExpression> : DocumentMergeProvider<HtmlDocument, HtmlNode, TSourceType, TSource, TExpression>
		where TSourceType : class
		where TExpression : class
	{
		public HtmlDocumentMergeProvider(ITokenParser<TSourceType> tokenParser, string tokenStart, string tokenEnd, IExpressionParser<TSourceType, TExpression> expressionParser, IGeneratorFactory<IGenerator<HtmlDocument, HtmlNode, TSourceType, TSource, TExpression>> generatorFactory, IDataProvider<TSourceType, TSource, TExpression> dataProvider, char escapeCharacter = '\0', bool strict = false)
			: this (new HtmlDocumentAdapter(), tokenStart, tokenEnd, tokenParser, expressionParser, generatorFactory, dataProvider, escapeCharacter, strict)
		{
		}

		private HtmlDocumentMergeProvider(IDocumentAdapter<HtmlDocument, HtmlNode> adapter, string tokenStart, string tokenEnd, ITokenParser<TSourceType> tokenParser, IExpressionParser<TSourceType, TExpression> expressionParser, IGeneratorFactory<IGenerator<HtmlDocument, HtmlNode, TSourceType, TSource, TExpression>> generatorFactory, IDataProvider<TSourceType, TSource, TExpression> dataProvider, char escapeCharacter = '\0', bool strict = false)
			: base(adapter, new DocumentTextScanner<HtmlDocument, HtmlNode>(adapter, tokenStart, tokenEnd, escapeCharacter, strict), tokenParser, expressionParser, generatorFactory, dataProvider)
		{
		}
	}
}
