using System.Collections.Generic;
using Aspose.Words;
using ExoMerge.Analysis;
using ExoMerge.DataAccess;
using ExoMerge.Rendering;

namespace ExoMerge.Aspose.MergeFields
{
	/// <summary>
	/// An object which can be used to produce a merged document from a given document template and data context, using fields of type 'MERGEFIELD'.
	/// </summary>
	/// <typeparam name="TSourceType">The type that represents the type of source data, e.g. 'Type'.</typeparam>
	/// <typeparam name="TSource">The type of the source data to merge, e.g. 'Object'.</typeparam>
	public class MergeFieldDocumentMergeProvider<TSourceType, TSource> : DocumentMergeProvider<TSourceType, TSource, string>
		where TSourceType : class
	{
		/// <summary>
		/// Creates a new merge provider.
		/// </summary>
		/// <param name="tokenParser">The object to use to parse the tokens' text.</param>
		/// <param name="expressionParser">The object to use to parse the raw expression text.</param>
		/// <param name="generatorFactory">An object that can assign custom generators.</param>
		/// <param name="dataProvider">An object used to access source data while merging.</param>
		public MergeFieldDocumentMergeProvider(ITokenParser<TSourceType> tokenParser, IExpressionParser<TSourceType, string> expressionParser, IGeneratorFactory<IGenerator<Document, Node, TSourceType, TSource, string>> generatorFactory, IDataProvider<TSourceType, TSource, string> dataProvider)
			: base(new MergeFieldScanner(), tokenParser, expressionParser, generatorFactory, dataProvider)
		{
		}

		/// <summary>
		/// Gets a value for the given expression. The merge field formatting switches
		/// are extracted from the expression, then used to format the resulting value.
		/// </summary>
		protected override string GetStandardFieldValue(DataContext<TSource, string> context, string expression, KeyValuePair<string, string>[] options)
		{
			var rawValue = DataProvider.GetValue(context, expression);

			return MergeFieldFormatter.ApplyFormats(rawValue, options);
		}
	}
}
