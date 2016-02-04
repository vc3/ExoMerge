using System.Collections.Generic;

namespace ExoMerge.Structure
{
	/// <summary>
	/// A region in a document marked by start and and tokens, which may container 'inner' tokens.
	/// </summary>
	/// <remarks>
	/// A 'region' is a spacial concept that deals with document structure in terms of tokens.
	/// Not all regions can directly contain other regions and fields, so a separate class
	/// <see cref="Container{TTarget,TElement,TToken,TSourceType,TSource,TExpression}"/> is used for that purpose.
	/// </remarks>
	/// <typeparam name="TToken">The type of token that marks the start and end of the region.</typeparam>
	public interface IRegion<out TToken>
	{
		/// <summary>
		/// The token that marks the start of the region.
		/// </summary>
		TToken StartToken { get; }

		/// <summary>
		/// Indicates whether the region owns its start token.
		/// </summary>
		bool OwnsStartToken { get; }

		/// <summary>
		/// The token that marks the end of the region.
		/// </summary>
		TToken EndToken { get; }

		/// <summary>
		/// Indicates whether the region owns its end token.
		/// </summary>
		bool OwnsEndToken { get; }

		/// <summary>
		/// Gets the tokens between the region's start and end tokens.
		/// </summary>
		IEnumerable<TToken> GetInnerTokens();
	}

	/// <summary>
	/// A region in a document marked by start and and tokens, which may container 'inner' tokens.
	/// </summary>
	/// <typeparam name="TToken">The type of token that marks the start and end of the region.</typeparam>
	internal interface IBuildableRegion<TToken> : IRegion<TToken>
	{
		/// <summary>
		/// Sets the token that marks the end of the region.
		/// </summary>
		void End(TToken token, bool ownsToken);
	}
}
