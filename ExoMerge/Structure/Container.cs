using System;
using System.Collections.Generic;

namespace ExoMerge.Structure
{
	/// <summary>
	/// An object that contains child objects.
	/// </summary>
	/// <typeparam name="TTarget">The type of object that the container's rendered content will apply to.</typeparam>
	/// <typeparam name="TElement">The type that is used to represent a discrete element in the target.</typeparam>
	/// <typeparam name="TToken">The type that represents a token.</typeparam>
	/// <typeparam name="TSourceType">The type that identifies the type of the data source, e.g. 'Type'.</typeparam>
	/// <typeparam name="TSource">The type of the data source that the container's content will be rendered for, e.g. 'Type'.</typeparam>
	/// <typeparam name="TExpression">The type of parsed expressions.</typeparam>
	public class Container<TTarget, TElement, TToken, TSourceType, TSource, TExpression>
		where TToken : class
		where TExpression : class
	{
		protected internal Container(TSourceType sourceType)
		{
			SourceType = sourceType;
			Children = new List<object>();
		}

		/// <summary>
		/// The type of the data source that the container's regions and fields will evaluate against.
		/// </summary>
		public TSourceType SourceType { get; private set; }

		/// <summary>
		/// Gets the list of child elements.
		/// </summary>
		public List<object> Children { get; private set; }

		/// <summary>
		/// Append the given child region.
		/// </summary>
		/// <param name="childRegion">The region to append.</param>
		internal void AppendChildRegion(IRegion<TToken> childRegion)
		{
			Children.Add(childRegion);
		}

		/// <summary>
		/// Append the given child field.
		/// </summary>
		/// <param name="childField">The field to append.</param>
		internal void AppendChildField(Field<TTarget, TElement, TToken, TSourceType, TSource, TExpression> childField)
		{
			Children.Add(childField);
		}

		/// <summary>
		/// Gets the tokens within the container.
		/// </summary>
		public IEnumerable<TToken> GetInnerTokens()
		{
			foreach (var child in Children)
			{
				if (child is IRegion<TToken>)
				{
					yield return ((IRegion<TToken>)child).StartToken;

					if (child is Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>)
						foreach (var childToken in ((Conditional<TTarget, TElement, TToken, TSourceType, TSource, TExpression>)child).GetInnerTokens())
							yield return childToken;
					else if (child is Repeatable<TTarget, TElement, TToken, TSourceType, TSource, TExpression>)
						foreach (var childToken in ((Repeatable<TTarget, TElement, TToken, TSourceType, TSource, TExpression>)child).GetInnerTokens())
							yield return childToken;
					else
						throw new Exception(string.Format("Unexpected child of type '{0}'.", child.GetType().Name));

					yield return ((IRegion<TToken>)child).EndToken;
				}
				else if (child is Field<TTarget, TElement, TToken, TSourceType, TSource, TExpression>)
					yield return ((Field<TTarget, TElement, TToken, TSourceType, TSource, TExpression>)child).Token;
				else
					throw new Exception(string.Format("Unexpected child element of type '{0}'.", child.GetType().Name));
			}
		}

	}
}
