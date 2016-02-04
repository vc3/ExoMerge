using System;
using ExoMerge.Analysis;
using ExoMerge.Structure;

namespace ExoMerge.Rendering
{
	/// <summary>
	/// A class that is responsible for manipulating an object of type <see cref="TTarget"/> during the merge process.
	/// </summary>
	/// <typeparam name="TTarget">The type of object that the merge writer will write to.</typeparam>
	/// <typeparam name="TElement">The type that is used to represent a discrete element in the target.</typeparam>
	/// <typeparam name="TToken">The type that represents tokens in the target object.</typeparam>
	public interface IMergeWriter<in TTarget, in TElement, TToken>
		where TToken : IToken<TElement, TElement>
	{
		/// <summary>
		/// Remove the given token from the target object.
		/// </summary>
		/// <param name="target">The object to remove the token from.</param>
		/// <param name="tokenToRemove">The token to remove.</param>
		void RemoveToken(TTarget target, TToken tokenToRemove);

		/// <summary>
		/// Replace the given token in the target object with the given object(s).
		/// </summary>
		/// <param name="target">The object in which to replace the given token.</param>
		/// <param name="tokenToReplace">The token to replace.</param>
		/// <param name="elements">The element(s) to insert in place of the given token.</param>
		void ReplaceToken(TTarget target, TToken tokenToReplace, params TElement[] elements);

		/// <summary>
		/// Remove nodes from the region defined by the given start and end tokens.
		/// </summary>
		/// <param name="target">The object in which to remove the given region's contents.</param>
		/// <param name="startToken">The token that marks the start of the region.</param>
		/// <param name="endToken">The token that marks the end of the region.</param>
		/// <param name="nodes">The region nodes to remove.</param>
		/// <param name="preserveTable">When removing nodes within a table, remove contents but not the table nodes themselves (i.e. rows, cells).</param>
		void RemoveRegionNodes(TTarget target, TToken startToken, TToken endToken, RegionNodes nodes, bool preserveTable = false);

		/// <summary>
		/// Clone the region identified by the given tokens.
		/// </summary>
		/// <param name="target">The object in which to clone the given region.</param>
		/// <param name="existingTokens">The tokens that identify the region to clone: Item1 is the start token, Item2 is the region's inner tokens, and Item3 is the end token.</param>
		/// <returns>The new cloned tokens, in the same format as the <see cref="existingTokens"/> parameter.</returns>
		Tuple<TToken, TToken[], TToken> CloneRegion(TTarget target, Tuple<TToken, TToken[], TToken> existingTokens);
	}
}
