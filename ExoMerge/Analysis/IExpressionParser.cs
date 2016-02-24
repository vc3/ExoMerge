using JetBrains.Annotations;
using System;

namespace ExoMerge.Analysis
{
	/// <summary>
	/// Parses expression text as an expression object.
	/// </summary>
	/// <typeparam name="TType">The type of the source type and result type of parsed expressions, e.g. 'Type'.</typeparam>
	/// <typeparam name="TExpression">The type of parsed expressions.</typeparam>
	public interface IExpressionParser<TType, TExpression>
	{
		/// <summary>
		/// Attempt to parse the given expression.
		/// </summary>
		/// <remarks>
		/// An implementing class should make a "best effort" to parse the given expression text and return as much
		/// information as it can.
		/// 
		/// ## Null Return Value
		/// 
		/// * A return value of null is considered an error.
		/// 
		/// ## Exceptions
		/// 
		/// * In most cases a parser should not catch exceptions unless it is able to provide better context about the
		///   exception than down-stream code would be able to derive from the raw exception.
		/// 
		/// </remarks>
		/// <param name="sourceType">The data source type.</param>
		/// <param name="text">The expression text to parse.</param>
		/// <returns>The parsed expression.</returns>
		TExpression Parse(TType sourceType, string text, Type resultType);

		/// <summary>
		/// Returns the result type of the given expression.
		/// </summary>
		TType GetResultType([NotNull] TExpression expression);
	}
}
