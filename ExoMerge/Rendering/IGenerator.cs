using System.Collections.Generic;
using ExoMerge.DataAccess;
using System;

namespace ExoMerge.Rendering
{
	/// <summary>
	/// An object that can provide arbitrary content for an expression.
	/// </summary>
	/// <typeparam name="TTarget">The type of object that the generated content will apply to.</typeparam>
	/// <typeparam name="TContent">The type of content that will be generated.</typeparam>
	/// <typeparam name="TSourceType">The type that identifies the type of the data source, e.g. 'Type'.</typeparam>
	/// <typeparam name="TSource">The type of the source data to merge, e.g. 'Object'.</typeparam>
	/// <typeparam name="TExpression">The type of parsed expressions.</typeparam>
	public interface IGenerator<in TTarget, out TContent, in TSourceType, TSource, TExpression>
		where TExpression : class
	{
		Type ExpectedType { get; }

		/// <summary>
		/// Generate content for the given expression.
		/// </summary>
		/// <param name="target">The target object that the content should be generated for.</param>
		/// <param name="dataProvider">An object that can be used to evaluate an expression against source data.</param>
		/// <param name="context">The current data context.</param>
		/// <param name="expression">The expression to generate content for.</param>
		/// <param name="options">An array of options.</param>
		/// <returns>Some number of content objects.</returns>
		IEnumerable<TContent> GenerateContent(TTarget target, IDataProvider<TSourceType, TSource, TExpression> dataProvider, DataContext<TSource, TExpression> context, TExpression expression, KeyValuePair<string, string>[] options);
	}
}
