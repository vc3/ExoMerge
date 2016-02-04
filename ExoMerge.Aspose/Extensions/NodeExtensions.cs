using System.Collections.Generic;
using Aspose.Words;
using JetBrains.Annotations;

namespace ExoMerge.Aspose.Extensions
{
	public static class NodeExtensions
	{
		/// <summary>
		/// Returns an enumeration of sibling nodes of the given node that
		/// follow the given node in the document.
		/// </summary>
		/// <param name="startingNode">The node to start after.</param>
		/// <returns>An enumeration of nodes that are siblings that follow the given node.</returns>
		public static IEnumerable<Node> GetFollowingSiblings([NotNull] this Node startingNode)
		{
			for (var node = startingNode.NextSibling; node != null; node = node.NextSibling)
				yield return node;
		}

		/// <summary>
		/// Returns an enumeration of the given node and its sibling nodes that
		/// follow it in the document.
		/// </summary>
		/// <param name="startingNode">The node to start at.</param>
		/// <returns>An enumeration of the given node and its siblings that follow it in the document.</returns>
		public static IEnumerable<Node> GetSelfAndFollowingSiblings([NotNull] this Node startingNode)
		{
			yield return startingNode;

			foreach (var node in startingNode.GetFollowingSiblings())
				yield return node;
		}
	}
}
