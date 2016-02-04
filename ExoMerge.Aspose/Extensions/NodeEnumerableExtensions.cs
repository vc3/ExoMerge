using System;
using System.Collections.Generic;
using Aspose.Words;
using JetBrains.Annotations;

namespace ExoMerge.Aspose.Extensions
{
	public static class NodeEnumerableExtensions
	{
		/// <summary>
		/// Returns nodes until the given node is found.
		/// </summary>
		public static IEnumerable<TNode> TakeUntilNode<TNode>(this IEnumerable<TNode> nodes, [NotNull] TNode firstNodeToExclude)
			where TNode : Node
		{
			var foundEnd = false;

			foreach (var node in nodes)
			{
				if (node == firstNodeToExclude)
				{
					foundEnd = true;
					break;
				}

				yield return node;
			}

			if (!foundEnd)
				throw new ArgumentOutOfRangeException("firstNodeToExclude");
		}

		/// <summary>
		/// Return nodes up to, and including, the given node.
		/// </summary>
		public static IEnumerable<TNode> TakeUpToNode<TNode>(this IEnumerable<TNode> nodes, [NotNull] TNode lastNodeToInclude)
			where TNode : Node
		{
			var foundEnd = false;

			foreach (var node in nodes)
			{
				yield return node;

				if (node == lastNodeToInclude)
				{
					foundEnd = true;
					break;
				}
			}

			if (!foundEnd)
				throw new ArgumentOutOfRangeException("lastNodeToInclude");
		}
	}
}
