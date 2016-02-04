using JetBrains.Annotations;

namespace ExoMerge.Structure
{
	/// <summary>
	/// Represents a repeatable region in a document.
	/// </summary>
	/// <typeparam name="TTarget">The type of object that the repeatable's rendered content will apply to.</typeparam>
	/// <typeparam name="TElement">The type that is used to represent a discrete element in the target.</typeparam>
	/// <typeparam name="TToken">The type that represents tokens in the document.</typeparam>
	/// <typeparam name="TSourceType">The type that identifies the type of the data source, e.g. 'Type'.</typeparam>
	/// <typeparam name="TSource">The type of the data source that the repeatable's content will be rendered for, e.g. 'Type'.</typeparam>
	/// <typeparam name="TExpression">The type of parsed expressions.</typeparam>
	public class Repeatable<TTarget, TElement, TToken, TSourceType, TSource, TExpression> : Container<TTarget, TElement, TToken, TSourceType, TSource, TExpression>, IBuildableRegion<TToken>
		where TToken : class
		where TExpression : class
	{
		internal Repeatable([NotNull] TToken startToken, [CanBeNull] TExpression expression, [CanBeNull] TSourceType itemType)
			: base(itemType)
		{
			StartToken = startToken;
			OwnsStartToken = true;
			Expression = expression;
		}

		internal Repeatable([NotNull] TToken startToken, [NotNull] TToken endToken, [CanBeNull] TExpression expression, [CanBeNull] TSourceType itemType)
			: base(itemType)
		{
			StartToken = startToken;
			OwnsStartToken = true;
			EndToken = endToken;
			OwnsEndToken = true;
			Expression = expression;
		}

		/// <summary>
		/// The token that marks the start of the repeatable.
		/// </summary>
		[NotNull]
		public TToken StartToken { get; private set; }

		/// <summary>
		/// Indicates whether the region owns its start token.
		/// </summary>
		public bool OwnsStartToken { get; private set; }

		/// <summary>
		/// The token that marks the end of the repeatable.
		/// </summary>
		public TToken EndToken { get; private set; }

		/// <summary>
		/// Indicates whether the region owns its end token.
		/// </summary>
		public bool OwnsEndToken { get; private set; }

		/// <summary>
		/// The repeatable expression.
		/// </summary>
		[CanBeNull]
		public TExpression Expression { get; private set; }

		/// <summary>
		/// Sets the token that marks the end of the region.
		/// </summary>
		void IBuildableRegion<TToken>.End(TToken token, bool ownsToken)
		{
			EndToken = token;
			OwnsEndToken = ownsToken;
		}
	}
}
