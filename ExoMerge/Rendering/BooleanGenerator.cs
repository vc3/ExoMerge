using System.Collections.Generic;
using ExoMerge.DataAccess;
using ExoMerge.Helpers;
using System;

namespace ExoMerge.Rendering
{
	/// <summary>
	/// Generates content, depending on whether the expression evalutes to a "truthy" value.
	/// </summary>
	public abstract class BooleanGenerator<TResult, TContent, TSourceType, TSource, TExpression> : IGenerator<TResult, TContent, TSourceType, TSource, TExpression>
		where TExpression : class
	{
		/// <summary>
		/// Gets the type of data that the generator's expression should return.
		/// </summary>
		public Type ExpectedType { get { return null; } }

		/// <summary>
		/// Converts the given value to a boolean.
		/// </summary>
		protected virtual bool ConvertToBoolean(object value)
		{
			return ValueConverter.ToBoolean(value);
		}

		/// <summary>
		/// Generates content for the given boolean value.
		/// </summary>
		protected abstract IEnumerable<TContent> GenerateContent(TResult target, bool value);

		/// <summary>
		/// Generate content for the given expression.
		/// </summary>
		public IEnumerable<TContent> GenerateContent(TResult target, IDataProvider<TSourceType, TSource, TExpression> dataProvider, DataContext<TSource, TExpression> context, TExpression expression, KeyValuePair<string, string>[] options)
		{
			var value = dataProvider.GetValue(context, expression);
			var booleanValue = ConvertToBoolean(value);

			return GenerateContent(target, booleanValue);
		}
	}
}
