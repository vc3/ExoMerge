using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using ExoMerge.Analysis;
using ExoMerge.DataAccess;
using ExoMerge.Documents;
using ExoMerge.Rendering;

namespace ExoMerge.OpenXml
{
	public class DocumentMergeProvider<TSourceType, TSource, TExpression> : DocumentMergeProvider<WordprocessingDocument, OpenXmlElement, TSourceType, TSource, TExpression>
		where TSourceType : class
		where TExpression : class
	{
		public DocumentMergeProvider(string tokenStart, string tokenEnd, ITokenParser<TSourceType> tokenParser, IExpressionParser<TSourceType, TExpression> expressionParser, IGeneratorFactory<IGenerator<WordprocessingDocument, OpenXmlElement, TSourceType, TSource, TExpression>> generatorFactory, IDataProvider<TSourceType, TSource, TExpression> dataProvider)
			: this(new DocumentAdapter(), tokenStart, tokenEnd, tokenParser, expressionParser, generatorFactory, dataProvider)
		{
		}

		public DocumentMergeProvider(IDocumentAdapter<WordprocessingDocument, OpenXmlElement> adapter, string tokenStart, string tokenEnd, ITokenParser<TSourceType> tokenParser, IExpressionParser<TSourceType, TExpression> expressionParser, IGeneratorFactory<IGenerator<WordprocessingDocument, OpenXmlElement, TSourceType, TSource, TExpression>> generatorFactory, IDataProvider<TSourceType, TSource, TExpression> dataProvider)
			: base(adapter, new DocumentTextScanner<WordprocessingDocument, OpenXmlElement>(adapter, tokenStart, tokenEnd), tokenParser, expressionParser, generatorFactory, dataProvider)
		{
		}
	}
}
