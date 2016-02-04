using System.Collections.Generic;
using ExoMerge.Rendering;

namespace ExoMerge.Structure
{
	/// <summary>
	/// Represents a content field in a document.
	/// </summary>
	/// <typeparam name="TTarget">The type of object that the field's content will apply to.</typeparam>
	/// <typeparam name="TElement">The type that is used to represent a discrete element in the target.</typeparam>
	/// <typeparam name="TToken">The type of token that marks a field.</typeparam>
	/// <typeparam name="TSourceType">The type that identifies the type of the data source, e.g. 'Type'.</typeparam>
	/// <typeparam name="TSource">The type of the source data to merge, e.g. 'Object'.</typeparam>
	/// <typeparam name="TExpression">The type of parsed expressions.</typeparam>
	public class Field<TTarget, TElement, TToken, TSourceType, TSource, TExpression>
		where TExpression : class
	{
		/// <summary>
		/// Creates a new field with the given arguments.
		/// </summary>
		/// <param name="token">The token that marks the field.</param>
		/// <param name="expression">The field's expression.</param>
		/// <param name="options">The field's options.</param>
		internal Field(TToken token, TExpression expression, KeyValuePair<string, string>[] options)
		{
			Token = token;
			Expression = expression;
			Options = options;
		}

		/// <summary>
		/// Gets the token that marks the field.
		/// </summary>
		public TToken Token { get; private set; }

		/// <summary>
		/// Gets the generator that should be used to generate content for the field.
		/// </summary>
		public IGenerator<TTarget, TElement, TSourceType, TSource, TExpression> Generator { get; internal set; }

		/// <summary>
		/// Gets the field's expression.
		/// </summary>
		public TExpression Expression { get; private set; }

		/// <summary>
		/// Gets the field's options.
		/// </summary>
		public KeyValuePair<string, string>[] Options { get; private set; }
	}
}
