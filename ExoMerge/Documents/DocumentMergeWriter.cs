using System;
using System.Collections.Generic;
using System.Linq;
using ExoMerge.Analysis;
using ExoMerge.Documents.Extensions;
using ExoMerge.Rendering;
using ExoMerge.Structure;

namespace ExoMerge.Documents
{
	/// <summary>
	/// A class that is responsible for manipulating a template during the merge process.
	/// </summary>
	/// <typeparam name="TDocument">The type of document to manipulate.</typeparam>
	/// <typeparam name="TNode">The type that represents nodes in the document.</typeparam>
	public class DocumentMergeWriter<TDocument, TNode> : IMergeWriter<TDocument, TNode, DocumentToken<TNode>>
		where TDocument : class
		where TNode : class
	{
		/// <summary>
		/// Creates a new document writer that uses the given adapter to access and manipulate a document.
		/// </summary>
		/// <param name="adapter">The adapter that will be used to access and manipulate a document.</param>
		public DocumentMergeWriter(IDocumentAdapter<TDocument, TNode> adapter)
		{
			Adapter = adapter;
		}

		/// <summary>
		/// Gets the adapter that will be used to access and manipulate a document.
		/// </summary>
		public IDocumentAdapter<TDocument, TNode> Adapter { get; private set; }

		/// <summary>
		/// Clone the given token's nodes and return a new token using those nodes.
		/// </summary>
		private DocumentToken<TNode> CloneToken(IToken<TNode, TNode> tokenToClone, TNode insertInto, TNode insertAfter, TNode insertBefore)
		{
			var startToClone = tokenToClone.Start;
			var endToClone = tokenToClone.End;

			var newStartNode = Adapter.Clone(startToClone, insertInto, insertAfter, insertBefore);

			insertAfter = newStartNode;

			TNode newEndNode;

			var newStartNodeAsEnd = newStartNode;

			if (newStartNodeAsEnd != null && startToClone == endToClone)
				newEndNode = newStartNodeAsEnd;
			else
			{
				for (var node = Adapter.GetNextSibling(startToClone); node != null && node != endToClone; node = Adapter.GetNextSibling(node))
				{
					var newNode = Adapter.Clone(node, insertInto, insertAfter, insertBefore);
					insertAfter = newNode;
				}

				newEndNode = Adapter.Clone(endToClone, insertInto, insertAfter, insertBefore);
			}

			return new DocumentToken<TNode>(newStartNode, newEndNode, tokenToClone.Value);
		}

		/// <summary>
		/// Get the next node in the document from the given node using depth-first traversal.
		/// * If the given node is a composite node, then return its first child if it has one.
		/// * If it is not a composite or is empty, then return its next sibling if it has one.
		/// * Otherwise, since the node is the last node in it's parent, step out and over to
		///   the parent's next sibling, unless the end of the document or the given container
		///   node is reached.
		/// </summary>
		private bool GetNextNode(TNode fromNode, TNode container, out TNode nextNode)
		{
			if (fromNode == null)
			{
				nextNode = null;
				return false;
			}

			// If a composite node is given as the "from" node, then visit its first child next.
			if (Adapter.IsComposite(fromNode) && Adapter.GetFirstChild(fromNode) != null)
			{
				nextNode = Adapter.GetFirstChild(fromNode);
				return true;
			}

			var node = fromNode;

			do
			{
				if (Adapter.GetNextSibling(node) != null)
				{
					nextNode = Adapter.GetNextSibling(node);
					return true;
				}

				node = Adapter.GetParent(node);

				if (node == container)
				{
					// If we have stepped out to the container,
					// then there are no more nodes to traverse.
					nextNode = null;
					return false;
				}

			} while (node != null);

			throw new Exception("The given node is not within the given container.");
		}

		/// <summary>
		/// Remove nodes from the document, starting and ending with the given nodes.
		/// </summary>
		private void RemoveNodes(TNode startNode, TNode endNode, bool removeParentIfEmpty)
		{
			TNode node = startNode;

			var parent = Adapter.GetParent(node);

			while (node != null)
			{
				var nextNode = Adapter.GetNextSibling(node);

				Adapter.Remove(node);

				if (node == endNode)
					break;

				node = nextNode;
			}

			if (removeParentIfEmpty && !Adapter.GetChildren(parent).Any())
				Adapter.Remove(parent);
		}

		/// <summary>
		/// Get the ancestor composite node that is common to both of the given nodes.
		/// </summary>
		private TNode GetCommonAncestor(TNode left, TNode right, bool allowDifferentLevels)
		{
			TNode leftDistinctAncestor;
			TNode rightDistinctAncestor;
			return GetCommonAncestor(left, right, allowDifferentLevels, out leftDistinctAncestor, out rightDistinctAncestor);
		}

		/// <summary>
		/// Get the ancestor composite node that is common to both of the given nodes.
		/// </summary>
		private TNode GetCommonAncestor(TNode left, TNode right, bool allowDifferentLevels, out TNode leftDistinctAncestor, out TNode rightDistinctAncestor)
		{
			if (left == null) throw new ArgumentNullException("left");
			if (right == null) throw new ArgumentNullException("right");

			var leftAncestor = Adapter.GetParent(left);
			var rightAncestor = Adapter.GetParent(right);

			if (leftAncestor == rightAncestor)
			{
				leftDistinctAncestor = null;
				rightDistinctAncestor = null;
				return leftAncestor;
			}

			List<TNode> leftAncestors = null;
			List<TNode> rightAncestors = null;

			if (allowDifferentLevels)
			{
				leftAncestors = new List<TNode> { leftAncestor };
				rightAncestors = new List<TNode> { rightAncestor };
			}

			while (Adapter.GetParent(leftAncestor) != null && Adapter.GetParent(rightAncestor) != null && Adapter.GetParent(leftAncestor) != Adapter.GetParent(rightAncestor))
			{
				if (allowDifferentLevels)
				{
					var indexOfLeftAncestorInRight = rightAncestors.IndexOf(Adapter.GetParent(leftAncestor));
					if (indexOfLeftAncestorInRight >= 0)
					{
						rightDistinctAncestor = indexOfLeftAncestorInRight == 0 ? null : rightAncestors[indexOfLeftAncestorInRight - 1];
						leftDistinctAncestor = leftAncestor;
						return Adapter.GetParent(leftAncestor);
					}

					var indexOfRightAncestorInLeft = leftAncestors.IndexOf(Adapter.GetParent(rightAncestor));
					if (indexOfRightAncestorInLeft >= 0)
					{
						leftDistinctAncestor = indexOfRightAncestorInLeft == 0 ? null : leftAncestors[indexOfRightAncestorInLeft - 1];
						rightDistinctAncestor = rightAncestor;
						return Adapter.GetParent(rightAncestor);
					}

					leftAncestors.Add(Adapter.GetParent(leftAncestor));
					rightAncestors.Add(Adapter.GetParent(rightAncestor));
				}

				leftAncestor = Adapter.GetParent(leftAncestor);
				rightAncestor = Adapter.GetParent(rightAncestor);
			}

			if (Adapter.GetParent(leftAncestor) == null || Adapter.GetParent(rightAncestor) == null)
			{
				leftDistinctAncestor = null;
				rightDistinctAncestor = null;
				return null;
			}

			leftDistinctAncestor = leftAncestor;
			rightDistinctAncestor = rightAncestor;
			return Adapter.GetParent(leftDistinctAncestor);
		}

		/// <summary>
		/// Determines if the given node is the only content in its parent row.
		/// </summary>
		private bool IsOnlyContentInRow(TNode node, out TNode row)
		{
			TNode ancestorCell = null;

			// Traverse up the heirarchy of nodes, while the node is
			// the only child of it's parent, until we find a cell.
			var commonAncestor = node;
			while (commonAncestor != null)
			{
				if (Adapter.GetNodeType(commonAncestor) == DocumentNodeType.TableCell)
				{
					ancestorCell = commonAncestor;
					break;
				}

				var parent = Adapter.GetParent(commonAncestor);

				if (parent == null)
					break;

				if (commonAncestor != Adapter.GetFirstChild(parent) || commonAncestor != Adapter.GetLastChild(parent))
					break;

				commonAncestor = parent;
			}

			if (ancestorCell != null)
			{
				// If the cell is the only cell in the row, then the row can be repeated.
				var parentRow = Adapter.GetParent(commonAncestor);
				if (ancestorCell == Adapter.GetFirstChild(parentRow) && ancestorCell == Adapter.GetLastChild(parentRow))
				{
					row = parentRow;
					return true;
				}
			}

			row = null;
			return false;
		}

		/// <summary>
		/// Determine if the given start and end nodes are at the beginning and end, respectively, of a row or range of rows.
		/// </summary>
		private bool MatchesRowBoundaries(TNode start, TNode end, out TNode startRow, out TNode endRow)
		{
			TNode startDistinctAncestor;
			TNode endDistinctAncestor;

			var commonAncestor = GetCommonAncestor(start, end, false, out startDistinctAncestor, out endDistinctAncestor);

			if (commonAncestor == null)
				throw new Exception("The start and end nodes aren't at the same document level.");

			if (Adapter.GetNodeType(startDistinctAncestor) == DocumentNodeType.TableCell && Adapter.GetNodeType(endDistinctAncestor) == DocumentNodeType.TableCell)
			{
				// If the distinct ancestors are cells, then see if they are the first and last cells in their row.

				var parentRow = commonAncestor;

				if (startDistinctAncestor != Adapter.GetFirstChild(parentRow))
					throw new Exception("The start token should be within the first cell in the row.");

				if (endDistinctAncestor != Adapter.GetLastChild(parentRow))
					throw new Exception("The end token should be within the last cell in the row.");

				startRow = endRow = commonAncestor;
				return true;
			}

			if (Adapter.GetNodeType(startDistinctAncestor) == DocumentNodeType.TableRow && Adapter.GetNodeType(endDistinctAncestor) == DocumentNodeType.TableRow)
			{
				startRow = startDistinctAncestor;
				endRow = endDistinctAncestor;

				if (Adapter.GetAncestor(start, DocumentNodeType.TableCell) != Adapter.GetFirstChild(startRow))
					throw new Exception("The start token should be within the first cell in the first row.");

				if (Adapter.GetAncestor(end, DocumentNodeType.TableCell) != Adapter.GetLastChild(endRow))
					throw new Exception("The end token should be within the last cell in the last row.");

				return true;
			}

			if (startDistinctAncestor == Adapter.GetFirstChild(commonAncestor) && endDistinctAncestor == Adapter.GetLastChild(commonAncestor))
			{
				// The start and end are the first and last children of their common ancestor,
				// so check to see if the common ancestor is the only content within a row.

				TNode row;
				if (IsOnlyContentInRow(commonAncestor, out row))
				{
					startRow = endRow = row;
					return true;
				}
			}

			startRow = null;
			endRow = null;
			return false;
		}

		/// <summary>
		/// Determine if the given start and end nodes are on the boundaries of one or more repeatable rows.
		/// </summary>
		private bool IsRowRepeatable(TNode start, TNode end, out TNode rowToClone)
		{
			var startParent = Adapter.GetParent(start);
			var endParent = Adapter.GetParent(end);

			// If the start and end share the same parent, then see if they are the
			// first and last child and and their parent is the only content in a row.
			if (startParent == endParent)
			{
				var parent = startParent;
				if (start != Adapter.GetFirstChild(parent) || end != Adapter.GetLastChild(parent))
				{
					rowToClone = null;
					return false;
				}

				return IsOnlyContentInRow(startParent, out rowToClone);
			}

			TNode startRow;
			TNode endRow;

			if (MatchesRowBoundaries(start, end, out startRow, out endRow))
			{
				rowToClone = startRow;
				return true;
			}

			rowToClone = null;
			return false;
		}

		private void CloneRegionTokens(DocumentToken<TNode> existingStartToken, DocumentToken<TNode>[] existingInnerTokens, DocumentToken<TNode> existingEndToken, out DocumentToken<TNode> newStartToken, out DocumentToken<TNode>[] newInnerTokens, out DocumentToken<TNode> newEndToken)
		{
			TNode commonAncestor;

			// Keep track of where cloned nodes should be inserted.

			TNode insertInto;
			TNode insertAfter;
			TNode insertBefore;

			TNode rowToClone;

			if (IsRowRepeatable(existingStartToken.Start, existingEndToken.End, out rowToClone))
			{
				commonAncestor = GetCommonAncestor(existingStartToken.Start, existingEndToken.End, false);

				// Clone the (start node's) containing row.
				var newRow = Adapter.CloneAndInsertBefore(rowToClone, rowToClone, false);

				var firstCell = Adapter.GetFirstChild(rowToClone);

				// Clone the start cell and append it to the new row.
				var newCell = Adapter.CloneAndAppend(firstCell, newRow, false);

				insertInto = newCell;

				// Clone the start nodes ancestors and ensure that the first non-composite node that is
				// encountered is the start node. The clone of the start node's immediate parent will be
				// the composite node to begin inserting cloned nodes into.

				TNode innerNode;
				TNode innerFromNode = firstCell;
				while (GetNextNode(innerFromNode, firstCell, out innerNode) && innerNode != existingStartToken.Start)
				{
					// If a non-composite node is encountered, or a composite node without any
					// children, then the start node is not the first node in the cell.
					var innerCompositeNode = Adapter.IsComposite(innerNode) ? innerNode : null;
					if (innerCompositeNode == null || !Adapter.GetChildren(innerCompositeNode).Any())
						throw new Exception("The token start node should be the first node in the table cell.");

					var newComposite = Adapter.CloneAndAppend(innerNode, insertInto, false);

					insertInto = newComposite;
					innerFromNode = innerNode;
				}

				if (innerNode != existingStartToken.Start)
					throw new Exception("Did not find the token start node within it's ancestor table cell.");

				insertAfter = null;
				insertBefore = null;
			}
			else
			{
				var startParent = Adapter.GetParent(existingStartToken.Start);
				var endParent = Adapter.GetParent(existingEndToken.Start);

				if (startParent == endParent)
				{
					// In the simple case where the start and end tokens are within the same parent node,
					// all that is needed is to insert each of the cloned nodes before the start token.

					insertInto = commonAncestor = startParent;
					insertAfter = null;
					insertBefore = existingStartToken.Start;
				}
				else
				{
					// If the start and end are in different containers, then we need to determine what
					// ancestor they share, and traverse up to that point in the document when cloning.

					commonAncestor = GetCommonAncestor(existingStartToken.Start, existingEndToken.End, false);

					if (commonAncestor == null)
						throw new Exception("The start and end nodes for the region aren't at the same document level.");

					// If the node is not the first node in it's parent, then start inserting
					// after it's preceding node (which will be moved to a new sub-graph).
					insertAfter = Adapter.GetPreviousSibling(existingStartToken.Start);

					// Split the nodes to the left of the start node into a new sub-graph.
					Adapter.SplitSubGraph(commonAncestor, existingStartToken.Start, out insertInto);

					// Since we moved the preceding nodes to a new ancestor, instead of inserting
					// cloned nodes before the existing start node we will now insert them **after**
					// the last sibling that was moved (if there was one).

					insertBefore = null;
				}
			}

			// Clone the start token and assign it to the new start token out parameter.

			newStartToken = CloneToken(existingStartToken, insertInto, insertAfter, insertBefore);
			insertAfter = newStartToken.End;

			// Keep track of the next token that should be encountered,
			// and the cloned nodes that will make up the cloned token.

			DocumentToken<TNode> nextToken;
			int nextTokenIndex;
			var newInnerTokensList = new List<DocumentToken<TNode>>();
			TNode newTokenStartNode = null;
			if (existingInnerTokens.Length > 0)
			{
				nextTokenIndex = 0;
				nextToken = existingInnerTokens[nextTokenIndex];
			}
			else
			{
				nextTokenIndex = -1;
				nextToken = null;
			}

			// Clone nodes until the existing end token is reached.

			TNode nodeToClone;
			TNode fromNode = existingStartToken.End;
			while (GetNextNode(fromNode, commonAncestor, out nodeToClone) && nodeToClone != existingEndToken.Start)
			{
				if (Adapter.IsComposite(nodeToClone))
				{
					// A composite node is visited in two cases: the last child of its previous sibling
					// was visited last, or its previous sibling was visited last and it had no children.

					var lastClonedNode = fromNode;

					// Determine what the last node was that was inserted into. If the last
					// cloned node was a composite, then it will be the insert into node, but
					// it hasn't actually been inserted into.
					TNode lastInsertedInto;
					if (Adapter.IsComposite(lastClonedNode))
						lastInsertedInto = Adapter.GetParent(insertInto);
					else
						lastInsertedInto = insertInto;

					if (Adapter.IsComposite(fromNode) && Adapter.GetParent(fromNode) == Adapter.GetParent(nodeToClone))
					{
						// Stepping up and over from a prior composite which was empty,
						// so start inserting after the prior node and into it's parent.
						// 
						//                        -                      (insert into) ->  lastInsertedInto
						//                      /   \                                         /      \
						//               fromNode   nodeToClone     (insert after) ->  insertInto      *
						//

						insertAfter = insertInto;
						insertInto = lastInsertedInto;
					}
					else if (Adapter.GetNextSibling(fromNode) == nodeToClone)
					{
						// Stepping up and over from a prior non-composite
						// (i.e. fromNode's next sibling is nodeToClone),
						// so continue the current insertion strategy.
						// 
						// NOTE: This is only possible if the document structure
						//       allows composite and non-composite nodes to be
						//       mixed in the same parent node.
						// 
						//                         -                         (insert into) ->  insertInto
						//                       /   \                                          /      \
						//                fromNode   nodeToClone     (insert after) ->  insertAfter      *
					}
					else if (Adapter.IsComposite(fromNode) && fromNode == Adapter.GetParent(nodeToClone))
					{
						// Stepping into a composite for the first time,
						// so continue the current insertion strategy.
						//
						//              fromNode      (insert into) ->  insertInto
						//               /                                /
						//        nodeToClone                            *
					}
					else
					{
						// Stepping out to visit an outer composite node for the first time,
						// so traverse up the heirarchy until we find the container.

						// Case where fromNode is an inline:
						//
						//                        container                       (insert into) ->  -
						//                         /     \                                        /   \
						//			        composite   nodeToClone    (insert after) ->  insertInto   *
						//                    /                                               /
						//              fromNode                                  insertAfter

						// Case where fromNode is a composite:
						//
						//                container                (insert into) ->  -
						//                 /     \                                 /   \
						//                -     nodeToClone    (insert after) ->  -     *
						//               /                                       /
						//     fromNode, composite                        insertInto

						var container = Adapter.GetParent(nodeToClone);

						// Since the insert into node is the last new composite, start iterating
						// at the nearest composite in the heirarchy of nodes to clone.
						var composite = Adapter.IsComposite(fromNode) ? fromNode : Adapter.GetParent(fromNode);

						while (composite != container)
						{
							insertAfter = insertInto;
							insertInto = Adapter.GetParent(insertInto);
							composite = Adapter.GetParent(composite);
						}
					}

					var newComposite = Adapter.Clone(nodeToClone, insertInto, insertAfter, null);

					insertBefore = null;
					insertAfter = null;
					insertInto = newComposite;
				}
				else
				{
					var parent = Adapter.GetParent(nodeToClone);

					if (fromNode != parent)
					{
						if (Adapter.GetParent(fromNode) != parent)
						{
							// Stepping out from an inline (fromNode) and into another inline (nodeToClone)
							// which is the next sibling of one of its (fromNode's) ancestors, so step out
							// to the appropriate node to insert into.
							//
							//              container                   (insert into) ->  -
							//               /    \                                     /   \
							//             -    nodeToClone   (insert after) ->  insertInto  *
							//            /                                           /
							//         fromNode                               insertAfter

							var container = Adapter.GetParent(nodeToClone);

							// Since the insert into node is the last new composite, start iterating
							// at the nearest composite in the heirarchy of nodes to clone.
							var composite = Adapter.IsComposite(fromNode) ? fromNode : Adapter.GetParent(fromNode);

							while (composite != container)
							{
								insertAfter = insertInto;
								insertInto = Adapter.GetParent(insertInto);
								composite = Adapter.GetParent(composite);
							}
						}
						else if (Adapter.IsComposite(fromNode) && !Adapter.GetChildren(fromNode).Any() && Adapter.GetParent(fromNode) == Adapter.GetParent(nodeToClone))
						{
							// Stepping over from a composite (fromNode) and into an inline (nodeToClone)
							// which is its next sibling, because the composite node is empty, so start
							// inserting into insertInto's parent and after insertInto.
							//
							//              container                      (insert into) ->  -
							//               /    \                                        /   \
							//          fromNode    nodeToClone   (insert after) -> insertInto  *
							//            /                                           /
							//

							insertAfter = insertInto;
							insertInto = Adapter.GetParent(insertInto);
						}
					}

					var newNode = Adapter.Clone(nodeToClone, insertInto, insertAfter, insertBefore);

					insertAfter = newNode;

					if (nextToken != null)
					{
						if (nodeToClone == nextToken.Start)
							newTokenStartNode = newNode;

						if (nodeToClone == nextToken.End)
						{
							if (newTokenStartNode == null)
								throw new Exception("Found token end node before start node.");

							newInnerTokensList.Add(new DocumentToken<TNode>(newTokenStartNode, newNode, nextToken.Value));
							nextToken = ++nextTokenIndex < existingInnerTokens.Length ? existingInnerTokens[nextTokenIndex] : null;
							newTokenStartNode = null;
						}
					}
				}

				fromNode = nodeToClone;
			}

			newEndToken = CloneToken(existingEndToken, insertInto, insertAfter, insertBefore);

			if (existingInnerTokens.Length > newInnerTokensList.Count)
				throw new Exception("Did not find all of the expected tokens after cloning.");

			if (existingInnerTokens.Length < newInnerTokensList.Count)
				throw new Exception("Found more than the expected number of tokens after cloning.");

			newInnerTokens = newInnerTokensList.ToArray();
		}

		/// <summary>
		/// Determine if the given composite is an ancestor of the given node.
		/// </summary>
		private bool IsAncestor(TNode composite, TNode node, TNode stopAtNode = null)
		{
			for (var parent = Adapter.GetParent(node); parent != null && (stopAtNode == null || parent != stopAtNode); parent = Adapter.GetParent(parent))
			{
				if (parent == composite)
					return true;
			}

			return false;
		}

		void IMergeWriter<TDocument, TNode, DocumentToken<TNode>>.RemoveToken(TDocument document, DocumentToken<TNode> tokenToRemove)
		{
			RemoveNodes(tokenToRemove.Start, tokenToRemove.End, true);
		}

		void IMergeWriter<TDocument, TNode, DocumentToken<TNode>>.ReplaceToken(TDocument document, DocumentToken<TNode> tokenToReplace, params TNode[] replacementNodes)
		{
			foreach (var newNode in replacementNodes)
				Adapter.InsertBefore(newNode, tokenToReplace.Start);

			RemoveNodes(tokenToReplace.Start, tokenToReplace.End, false);
		}

		void IMergeWriter<TDocument, TNode, DocumentToken<TNode>>.RemoveRegionNodes(TDocument document, DocumentToken<TNode> startToken, DocumentToken<TNode> endToken, RegionNodes nodes, bool preserveTable)
		{
			if (nodes.HasFlag(RegionNodes.Content))
			{
				TNode startDistinctAncestor;
				TNode endDistinctAncestor;

				var commonAncestor = GetCommonAncestor(startToken.End, endToken.Start, false, out startDistinctAncestor, out endDistinctAncestor);

				var encounteredStartNodeAncestor = false;

				var encounteredEndNodeAncestor = false;

				var preserveTopLevelRows = false;

				var preserveTopLevelCells = false;

				if (preserveTable)
				{
					switch (Adapter.GetNodeType(commonAncestor))
					{
						case DocumentNodeType.Table:
							preserveTopLevelRows = true;
							break;
						case DocumentNodeType.TableRow:
							preserveTopLevelCells = true;
							break;
					}
				}

				TNode node;
				TNode fromNode = startToken.End;
				while (GetNextNode(fromNode, commonAncestor, out node))
				{
					if (node == endToken.Start)
						break;

					// When navigating from one composite node to another, the new composite node
					// will be returned before any of its children. If the composite is an ancestor
					// of the start or end node, then don't remove it and instead start getting the
					// next node from that composite node in order to remove any following siblings
					// of the start node or following siblings of its ancestors, or preceding
					// siblings of the end node or preceding siblings of its ancestors.
					if (node == startDistinctAncestor)
					{
						encounteredStartNodeAncestor = true;
						fromNode = node;
					}
					else if (node == endDistinctAncestor)
					{
						encounteredEndNodeAncestor = true;
						fromNode = node;
					}
					// After encountering the start or end nodes' distinct ancestor, if a node is a composite,
					// then continue without removing it if it is also an ancestor of the start or end node.
					else if (encounteredStartNodeAncestor && Adapter.IsComposite(node) && IsAncestor(node, startToken.End, stopAtNode: commonAncestor))
					{
						fromNode = node;
					}
					else if (encounteredEndNodeAncestor && Adapter.IsComposite(node) && IsAncestor(node, endToken.Start, stopAtNode: commonAncestor))
					{
						fromNode = node;
					}
					// Otherwise, remove the node if needed...
					else
					{
						if (preserveTopLevelRows)
						{
							// Don't remove the top-level rows...
							if (Adapter.GetNodeType(node) == DocumentNodeType.TableRow && Adapter.GetParent(node) == commonAncestor)
							{
								fromNode = node;
								continue;
							}

							// Don't remove the cells in the top-level rows...
							if (Adapter.GetNodeType(node) == DocumentNodeType.TableCell && Adapter.GetChildren(commonAncestor).Contains(Adapter.GetParent(node)))
							{
								fromNode = node;
								continue;
							}
						}

						if (preserveTopLevelCells)
						{
							// Don't remove top-level cells...
							if (Adapter.GetNodeType(node) == DocumentNodeType.TableCell && Adapter.GetParent(node) == commonAncestor)
							{
								fromNode = node;
								continue;
							}
						}

						Adapter.Remove(node);
					}
				}
			}

			if (nodes.HasFlag(RegionNodes.Start))
				RemoveNodes(startToken.Start, startToken.End, true);

			if (nodes.HasFlag(RegionNodes.End))
				RemoveNodes(endToken.Start, endToken.End, true);
		}

		Tuple<DocumentToken<TNode>, DocumentToken<TNode>[], DocumentToken<TNode>> IMergeWriter<TDocument, TNode, DocumentToken<TNode>>.CloneRegion(TDocument document, Tuple<DocumentToken<TNode>, DocumentToken<TNode>[], DocumentToken<TNode>> existingTokens)
		{
			DocumentToken<TNode> newStartToken;
			DocumentToken<TNode>[] newInnerTokens;
			DocumentToken<TNode> newEndToken;

			CloneRegionTokens(existingTokens.Item1, existingTokens.Item2, existingTokens.Item3, out newStartToken, out newInnerTokens, out newEndToken);

			return new Tuple<DocumentToken<TNode>, DocumentToken<TNode>[], DocumentToken<TNode>>(newStartToken, newInnerTokens, newEndToken);
		}
	}
}
