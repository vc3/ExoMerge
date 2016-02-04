namespace ExoMerge.Documents.Extensions
{
	/// <summary>
	/// Provides a set of static (Shared in Visual Basic) methods for manipulating a document using an implementation of 'IDocumentAdapter'.
	/// </summary>
	public static class DocumentAdapterExtensions
	{
		/// <summary>
		/// Clone the given node and insert it before the given 'insertBefore' node.
		/// </summary>
		public static T CloneAndInsertBefore<TDocument, TNode, T>(this IDocumentAdapter<TDocument, TNode> adapter, T node, TNode insertBefore, bool recursive)
			where T : TNode
		{
			var newNode = adapter.Clone(node, recursive);
			adapter.InsertBefore(newNode, insertBefore);
			return (T)newNode;
		}

		/// <summary>
		/// Clone the give node and insert it after the given 'insertAfter' node.
		/// </summary>
		public static T CloneAndInsertAfter<TDocument, TNode, T>(this IDocumentAdapter<TDocument, TNode> adapter, T node, TNode insertAfter, bool recursive)
			where T : TNode
		{
			var newNode = adapter.Clone(node, recursive);
			adapter.InsertAfter(newNode, insertAfter);
			return (T)newNode;
		}

		/// <summary>
		/// Clone the given node and append it to the given 'insertInto' composite node.
		/// </summary>
		public static T CloneAndAppend<TDocument, TNode, T>(this IDocumentAdapter<TDocument, TNode> adapter, T node, TNode insertInto, bool recursive)
			where T : TNode
		{
			var newNode = adapter.Clone(node, recursive);
			adapter.AppendChild(insertInto, newNode);
			return (T)newNode;
		}

		/// <summary>
		/// Clone the given node and insert it into the document after the given 'insertAfter' node,
		/// or before the given 'insertBefore' node, or into the given 'insertInto' node.
		/// </summary>
		public static T Clone<TDocument, TNode, T>(this IDocumentAdapter<TDocument, TNode> adapter, T node, TNode insertInto, TNode insertAfter, TNode insertBefore)
			where T : TNode
		{
			if (insertAfter != null)
				return adapter.CloneAndInsertAfter(node, insertAfter, false);

			if (insertBefore != null)
				return adapter.CloneAndInsertBefore(node, insertBefore, false);

			return adapter.CloneAndAppend(node, insertInto, false);
		}

		/// <summary>
		/// Move the given node's preceding siblings to a new container.
		/// </summary>
		private static void RelocatePrecedingSiblings<TDocument, TNode>(IDocumentAdapter<TDocument, TNode> adapter, TNode node, TNode newContainer)
			where TNode : class
		{
			TNode firstChild;

			while ((firstChild = adapter.GetFirstChild(adapter.GetParent(node))) != node)
			{
				// Move the first child to the new container.
				adapter.AppendChild(newContainer, firstChild);
			}
		}

		///  <summary>
		///  Split a subgraph into two adjacent subgraphs, pivoting on the given ancestor and moving nodes to the left of the
		///  given node into the new, prepended subgraph, so that the node is now the left-most node of the original subgraph.
		///  </summary>
		///  <example>
		///  
		///  ancestor = (a)
		///  node = (n1)
		///  
		///          (a)                         (a)
		///          / \                      /   |   \
		///        p1   p2         ->     (p1')   p1    p2
		///       /    /  \                       |     |  \
		///     (n1)  n2   n3                    (n1)   n2  n3
		/// 
		///  newContainer = p1'
		///  
		///  </example>
		///  <example>
		///  
		///  ancestor = (a)
		///  node = (n3)
		///  
		///          (a)                        (a)
		///          / \                     /   |   \
		///        p1   p2         ->      p1  (p2')  p2
		///       /    /  \               /     |      \
		///     n1    n2  (n3)           n1     n2     (n3)
		/// 
		///  newContainer = p2'
		///  
		///  </example>
		///  <example>
		///  
		///  ancestor = (a)
		///  node = (n4)
		///  
		///                 (a)                                (a)
		///                /   \                            /   |   \
		///            ggp1     ggp2                  ggp1'    ggp1  ggp2
		///            /            \         ->       /        |       \
		///         gp1              gp2              gp1'     gp1       gp2
		///        /   \            /   \            /  \       |       /   \
		///      p1     p2         p3    p4         p1  (p2')   p2     p3    p4
		///    / |      | \        |    /  \       / |    |     |      |    /  \
		///  n1  n2    n3  (n4)    n5  n6  n7     n1  n2  n3   (n4)    n5  n6  n7
		/// 
		///  newContainer = p2'
		///  
		///  </example>
		/// <param name="adapter">The document adapter.</param>
		/// <param name="ancestor">The ancestor node on which to pivot the split of the subgraph, which will be the common parent of the two resulting subgraphs.</param>
		///  <param name="node">The node at which to split the subgraph.</param>
		///  <param name="newContainer">The clone of the node's container that now resides in the new subgraph.</param>
		public static void SplitSubGraph<TDocument, TNode>(this IDocumentAdapter<TDocument, TNode> adapter, TNode ancestor, TNode node, out TNode newContainer)
			where TNode : class
		{
			var parent = adapter.GetParent(node);

			if (adapter.GetParent(parent) != ancestor)
			{
				TNode newGraphparent;

				adapter.SplitSubGraph(ancestor, parent, out newGraphparent);

				newContainer = adapter.CloneAndAppend(parent, newGraphparent, false);
			}
			else
			{
				// Clone the right node's container.
				newContainer = adapter.CloneAndInsertBefore(parent, parent, false);
			}

			// If the node is not the first node in its parent,
			// then move the preceding nodes to the new parent.

			RelocatePrecedingSiblings(adapter, node, newContainer);
		}
	}
}