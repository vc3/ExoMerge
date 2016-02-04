using System;
using ExoMerge.DataAccess;
using ExoModel;

namespace ExoMerge.ModelExpressions
{
	/// <summary>
	/// Provides access to data during the merge operation using model instances and model expressions.
	/// </summary>
	public class ModelExpressionDataProvider : IDataProvider<ModelType, IModelInstance, ModelExpression>
	{
		/// <summary>
		/// Gets the type of the given data source object.
		/// </summary>
		/// <param name="source">The source object to get the type of.</param>
		/// <returns>The type of the given data object.</returns>
		public ModelType GetSourceType(IModelInstance source)
		{
			return source.Instance.Type;
		}

		/// <summary>
		/// Evaluates the given expression for the given source object and returns the resulting value.
		/// </summary>
		/// <param name="context">The data context to evaluate the expression against.</param>
		/// <param name="expression">The expression to evaluate.</param>
		/// <returns>The result of evaluating the expression for the current source object.</returns>
		public virtual object GetValue(DataContext<IModelInstance, ModelExpression> context, ModelExpression expression)
		{
			return expression.GetValue(context.Source.Instance);
		}

		/// <summary>
		/// Evaluates the given expression for the given source object and returns the resulting value,
		/// formatted using the given format string.
		/// </summary>
		/// <param name="context">The data context to evaluate the expression against.</param>
		/// <param name="expression">The expression to evaluate.</param>
		/// <param name="format">The format string.</param>
		/// <param name="provider">The format provider.</param>
		/// <param name="rawValue">The unformatted value.</param>
		/// <returns>The result of evaluating the expression for the current source object.</returns>
		public virtual string GetFormattedValue(DataContext<IModelInstance, ModelExpression> context, ModelExpression expression, string format, IFormatProvider provider, out object rawValue)
		{
			return expression.GetFormattedValue(context.Source.Instance, format, provider, out rawValue);
		}
	}
}
