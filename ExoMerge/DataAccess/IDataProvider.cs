using System;
using JetBrains.Annotations;

namespace ExoMerge.DataAccess
{
	/// <summary>
	/// Provides access to data during the merge operation.
	/// </summary>
	/// <typeparam name="TSourceType">The type that represents the type of source data, e.g. 'Type'.</typeparam>
	/// <typeparam name="TSource">The type of the source data to merge, e.g. 'Object'.</typeparam>
	/// <typeparam name="TExpression">The type of parsed expressions.</typeparam>
	public interface IDataProvider<out TSourceType, TSource, TExpression>
		where TExpression : class
	{
		/// <summary>
		/// Gets the type of the given data source object.
		/// </summary>
		/// <param name="source">The source object to get the type of.</param>
		/// <returns>The type of the given data object.</returns>
		TSourceType GetSourceType(TSource source);

		/// <summary>
		/// Evaluates the given expression for the given source object and returns the resulting value.
		/// </summary>
		/// <param name="context">The data context to evaluate the expression against.</param>
		/// <param name="expression">The expression to evaluate.</param>
		/// <returns>The result of evaluating the expression for the current source object.</returns>
		object GetValue([NotNull] DataContext<TSource, TExpression> context, [NotNull] TExpression expression);

		/// <summary>
		/// Evaluates the given expression for the given source object and returns the resulting value,
		/// formatted using the given format string.
		/// </summary>
		/// <param name="context">The data context to evaluate the expression against.</param>
		/// <param name="expression">The expression to evaluate.</param>
		/// <param name="format">The format string.</param>
		/// <param name="provider">The format provider.</param>
		/// <returns>The result of evaluating the expression for the current source object.</returns>
		string GetFormattedValue([NotNull] DataContext<TSource, TExpression> context, [NotNull] TExpression expression, string format, IFormatProvider provider);
	}
}
