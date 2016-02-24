using System.Linq;
using ExoMerge.Analysis;
using ExoMerge.DataAccess;
using ExoMerge.Rendering;
using ExoMerge.Structure;

namespace ExoMerge.Documents
{
	/// <summary>
	/// An object which can be used to produce a merged document from a given document template and data context.
	/// </summary>
	/// <typeparam name="TDocument">The type of document used for the template and result.</typeparam>
	/// <typeparam name="TNode">The type that represents nodes in the document.</typeparam>
	/// <typeparam name="TSourceType">The type that represents the type of source data, e.g. 'Type'.</typeparam>
	/// <typeparam name="TSource">The type of the source data to merge, e.g. 'Object'.</typeparam>
	/// <typeparam name="TExpression">The type of parsed expressions.</typeparam>
	public class DocumentMergeProvider<TDocument, TNode, TSourceType, TSource, TExpression> : MergeProvider<TDocument, DocumentToken<TNode>, TNode, TSourceType, TSource, TExpression>
		where TDocument : class
		where TNode : class
		where TSourceType : class
		where TExpression : class
	{
		/// <summary>
		/// Creates a new merge provider.
		/// </summary>
		/// <param name="adapter">An object used to access and manipulate the document.</param>
		/// <param name="scanner">An object used to find tokens in the document.</param>
		/// <param name="tokenParser">The object to use to parse the tokens' text.</param>
		/// <param name="expressionParser">The object to use to parse the raw expression text.</param>
		/// <param name="generatorFactory">An object that can assign custom generators.</param>
		/// <param name="dataProvider">An object used to access source data while merging.</param>
		public DocumentMergeProvider(IDocumentAdapter<TDocument, TNode> adapter, ITemplateScanner<TDocument, DocumentToken<TNode>> scanner, ITokenParser<TSourceType> tokenParser, IExpressionParser<TSourceType, TExpression> expressionParser, IGeneratorFactory<IGenerator<TDocument, TNode, TSourceType, TSource, TExpression>> generatorFactory, IDataProvider<TSourceType, TSource, TExpression> dataProvider)
			: base(scanner, tokenParser, expressionParser, generatorFactory, dataProvider, new DocumentMergeWriter<TDocument, TNode>(adapter))
		{
			Adapter = adapter;
		}

		/// <summary>
		/// Gets the object used to access and manipulate the document.
		/// </summary>
		public IDocumentAdapter<TDocument, TNode> Adapter { get; private set; }

		/// <summary>
		/// Preserve an empty row (or rows) for a region's contents when the region is removed.
		/// </summary>
		public bool KeepEmptyRegionRows { get; set; }

		/// <summary>
		/// Determines if the node is empty or whitespace text, or if it is a container,
		/// that it has no children, or its children only contain whitespace.
		/// </summary>
		private bool IsEmptyOrWhitespace(TNode node)
		{
			if (Adapter.GetNodeType(node) == DocumentNodeType.Run)
				return string.IsNullOrWhiteSpace(Adapter.GetText(node));

			var children = Adapter.GetChildren(node).ToArray();
			if (children.Length == 0)
				return true;

			return Adapter.GetChildren(node).All(IsEmptyOrWhitespace);
		}

		/// <summary>
		/// Called when a cell's contents has been emptied during merge.
		/// </summary>
		protected virtual void OnCellEmptied(TNode cell)
		{
			var row = Adapter.GetAncestor(cell, DocumentNodeType.TableRow);

			// Determine if the cell is the only cell in the row...
			if (cell == Adapter.GetFirstChild(row) && cell == Adapter.GetLastChild(row))
				OnRowEmptied(row);
		}

		/// <summary>
		/// Called when a row's contents has been emptied during merge.
		/// </summary>
		/// <param name="row">The row that was emptied.</param>
		/// <param name="forceRemoval">True if the row should always be removed, whether or not the 'KeepEmptyRegionRows' option is enabled.</param>
		protected virtual void OnRowEmptied(TNode row, bool forceRemoval = false)
		{
			var parentTable = Adapter.GetAncestor(row, DocumentNodeType.Table);

			if (forceRemoval || !KeepEmptyRegionRows)
			{
				if (Adapter.GetChildren(parentTable).Count() == 1)
				{
					// This is the last row it the table, so remove the table.
					OnTableEmptied(parentTable);
				}
				else
				{
					// Otherwise, just remove the row.
					Adapter.Remove(row);
				}
			}
		}

		/// <summary>
		/// Called when a row's contents has been emptied during merge.
		/// </summary>
		protected virtual void OnTableEmptied(TNode table)
		{
			// Remove the table
			Adapter.Remove(table);
		}

		/// <summary>
		/// Remove the given region's tags from the document.
		/// </summary>
		internal override void RemoveRegionTags(TDocument document, IRegion<DocumentToken<TNode>> region)
		{
			var startParent = Adapter.GetParent(region.StartToken.Start);
			var endParent = Adapter.GetParent(region.EndToken.End);

			var startGrandparent = Adapter.GetParent(startParent);
			var endGrandparent = Adapter.GetParent(endParent);

			if (Adapter.GetNodeType(startGrandparent) == DocumentNodeType.TableCell && Adapter.GetNodeType(endGrandparent) == DocumentNodeType.TableCell)
			{
				var startCell = startGrandparent;
				var endCell = endGrandparent;

				var startRow = Adapter.GetAncestor(startCell, DocumentNodeType.TableRow);
				var startTable = Adapter.GetAncestor(startRow, DocumentNodeType.Table);

				var endRow = Adapter.GetAncestor(endCell, DocumentNodeType.TableRow);
				var endTable = Adapter.GetAncestor(endRow, DocumentNodeType.Table);

				RegionNodes nodesToRemove;

				if (region.OwnsStartToken && region.OwnsEndToken)
					nodesToRemove = RegionNodes.Start | RegionNodes.End;
				else if (region.OwnsStartToken)
					nodesToRemove = RegionNodes.Start;
				else if (region.OwnsEndToken)
					nodesToRemove = RegionNodes.End;
				else
					return;

				// The region's tags are within different single cells...
				if (startCell != endCell)
				{
					// The cells are in different rows...
					if (startRow != endRow)
					{
						// The rows are within the same table...
						if (startTable == endTable)
						{
							var removeStartRow = false;

							if (nodesToRemove.HasFlag(RegionNodes.Start))
							{
								var beforeStart = Adapter.GetPreviousSibling(region.StartToken.Start);
								var afterStart = Adapter.GetNextSibling(region.StartToken.End);

								// Determine if the start token is alone in its parent...
								if ((beforeStart == null || (Adapter.GetChildren(startParent).TakeWhile(n => n != region.StartToken.Start).All(IsEmptyOrWhitespace)))
								    && (afterStart == null || (Adapter.GetChildren(startParent).SkipWhile(n => n != region.StartToken.End).Skip(1).All(IsEmptyOrWhitespace))))
								{
									// Determine if the start token spans its entire cell...
									if ((startParent == Adapter.GetFirstChild(startCell) || Adapter.GetChildren(startCell).TakeWhile(n => n != startParent).All(IsEmptyOrWhitespace))
									    && (startParent == Adapter.GetLastChild(startCell) || Adapter.GetChildren(startCell).SkipWhile(n => n != startParent).Skip(1).All(IsEmptyOrWhitespace)))
									{
										// Determine if the cell is the only cell in the row, or all other cells are empty
										if ((startCell == Adapter.GetFirstChild(startRow) || Adapter.GetChildren(startRow).TakeWhile(n => n != startCell).All(IsEmptyOrWhitespace))
										    && (startCell == Adapter.GetLastChild(startRow) || Adapter.GetChildren(startRow).SkipWhile(n => n != startCell).Skip(1).All(IsEmptyOrWhitespace)))
										{
											removeStartRow = true;
										}
									}
								}
							}

							var removeEndRow = false;

							if (nodesToRemove.HasFlag(RegionNodes.End))
							{
								var beforeEnd = Adapter.GetPreviousSibling(region.EndToken.Start);
								var afterEnd = Adapter.GetNextSibling(region.EndToken.End);

								// Determine if the end token is alone in its parent...
								if ((beforeEnd == null || (Adapter.GetChildren(endParent).TakeWhile(n => n != region.EndToken.Start).All(IsEmptyOrWhitespace)))
								    && (afterEnd == null || (Adapter.GetChildren(endParent).SkipWhile(n => n != region.EndToken.End).Skip(1).All(IsEmptyOrWhitespace))))
								{
									// Determine if the end token spans its entire cell...
									if ((endParent == Adapter.GetFirstChild(endCell) || Adapter.GetChildren(endCell).TakeWhile(n => n != endParent).All(IsEmptyOrWhitespace))
									    && (endParent == Adapter.GetLastChild(endCell) || Adapter.GetChildren(endCell).SkipWhile(n => n != endParent).Skip(1).All(IsEmptyOrWhitespace)))
									{
										// Determine if the cell is the only cell in the row, or all other cells are empty
										if ((endCell == Adapter.GetFirstChild(endRow) || Adapter.GetChildren(endRow).TakeWhile(n => n != endCell).All(IsEmptyOrWhitespace))
										    && (endCell == Adapter.GetLastChild(endRow) || Adapter.GetChildren(endRow).SkipWhile(n => n != endCell).Skip(1).All(IsEmptyOrWhitespace)))
										{
											removeEndRow = true;
										}
									}
								}
							}

							if (removeStartRow || removeEndRow)
							{
								Writer.RemoveRegionNodes(document, region.StartToken, region.EndToken, nodesToRemove);

								if (removeStartRow)
									OnRowEmptied(startRow, forceRemoval: true);

								if (removeEndRow)
									OnRowEmptied(endRow, forceRemoval: true);

								return;
							}
						}
					}
				}
			}

			base.RemoveRegionTags(document, region);
		}

		/// <summary>
		/// Remove the given region from the document.
		/// </summary>
		internal override void RemoveRegion(TDocument document, IRegion<DocumentToken<TNode>> region)
		{
			var startParent = Adapter.GetParent(region.StartToken.Start);
			var endParent = Adapter.GetParent(region.EndToken.End);

			var startGrandparent = Adapter.GetParent(startParent);
			var endGrandparent = Adapter.GetParent(endParent);

			if (Adapter.GetNodeType(startGrandparent) == DocumentNodeType.TableCell && Adapter.GetNodeType(endGrandparent) == DocumentNodeType.TableCell)
			{
				var beforeStart = Adapter.GetPreviousSibling(region.StartToken.Start);
				var afterEnd = Adapter.GetNextSibling(region.EndToken.End);

				var startCell = startGrandparent;
				var endCell = endGrandparent;

				var startRow = Adapter.GetAncestor(startCell, DocumentNodeType.TableRow);
				var startTable = Adapter.GetAncestor(startRow, DocumentNodeType.Table);

				var endRow = Adapter.GetAncestor(endCell, DocumentNodeType.TableRow);
				var endTable = Adapter.GetAncestor(endRow, DocumentNodeType.Table);

				RegionNodes nodesToRemove;

				if (region.OwnsStartToken && region.OwnsEndToken)
					nodesToRemove = RegionNodes.Start | RegionNodes.End | RegionNodes.Content;
				else if (region.OwnsStartToken)
					nodesToRemove = RegionNodes.Start | RegionNodes.Content;
				else
				{
					base.RemoveRegion(document, region);
					return;
				}

				// If the region encompasses one or more table cells/rows, then potentially
				// remove one or more empty rows.

				// The region is within a single cell...
				if (startCell == endCell)
				{
					var cell = startCell;

					if (nodesToRemove.HasFlag(RegionNodes.End))
					{
						// Determine if the region starts and ends the cell...
						if (beforeStart == null && startParent == Adapter.GetFirstChild(cell)
						    && afterEnd == null && endParent == Adapter.GetLastChild(cell))
						{
							Writer.RemoveRegionNodes(document, region.StartToken, region.EndToken, nodesToRemove);
							OnCellEmptied(cell);
							return;
						}
					}
				}
				// The region's cells are in the same row...
				else if (startRow == endRow)
				{
					var row = startRow;

					if (nodesToRemove.HasFlag(RegionNodes.End))
					{
						// Determine if the region spans an entire row...
						if (beforeStart == null && startParent == Adapter.GetFirstChild(startCell) && startCell == Adapter.GetFirstChild(row))
						{
							if (afterEnd == null && endParent == Adapter.GetLastChild(endCell) && endCell == Adapter.GetLastChild(row))
							{
								Writer.RemoveRegionNodes(document, region.StartToken, region.EndToken, nodesToRemove);
								OnRowEmptied(row);
								return;
							}
						}
					}
					else
					{
						var beforeEnd = Adapter.GetPreviousSibling(region.EndToken.Start);

						// Determine if the region spans the entire cell...
						if (Adapter.GetNextSibling(startCell) == endCell
							&& (beforeStart == null && startParent == Adapter.GetFirstChild(startCell))
							&& (beforeEnd == null && endParent == Adapter.GetFirstChild(endCell)))
						{
							Writer.RemoveRegionNodes(document, region.StartToken, region.EndToken, nodesToRemove);
							OnCellEmptied(startCell);
							return;
						}
					}
				}
				// The region's cells are in rows in the same table...
				else if (startTable == endTable)
				{
					var spansEntireRows = false;

					// Determine if the regions spans the entirety of more than one row...
					if (beforeStart == null && startParent == Adapter.GetFirstChild(startCell) && startCell == Adapter.GetFirstChild(startRow))
					{
						if (nodesToRemove.HasFlag(RegionNodes.End))
						{
							// Determine if the end token is at the end of the end row...
							if (afterEnd == null && endParent == Adapter.GetLastChild(endCell) && endCell == Adapter.GetLastChild(endRow))
							{
								spansEntireRows = true;
							}
						}
						else
						{
							var beforeEnd = Adapter.GetPreviousSibling(region.EndToken.Start);

							// Determine if the end token is at the start of the end row...
							if (beforeEnd == null && endParent == Adapter.GetFirstChild(endCell) && endCell == Adapter.GetFirstChild(endRow))
							{
								spansEntireRows = true;
							}
						}
					}

					if (spansEntireRows)
					{
						var forceRemoveStartRow = false;
						var forceRemoveEndRow = false;

						// Always force remove rows that are just for tags...
						if (KeepEmptyRegionRows)
						{
							var afterStart = Adapter.GetNextSibling(region.StartToken.End);

							// Determine if the start token is alone in its parent...
							if ((beforeStart == null || (Adapter.GetChildren(startParent).TakeWhile(n => n != region.StartToken.Start).All(IsEmptyOrWhitespace)))
							    && (afterStart == null || (Adapter.GetChildren(startParent).SkipWhile(n => n != region.StartToken.End).Skip(1).All(IsEmptyOrWhitespace))))
							{
								// Determine if the start token spans its entire cell...
								if ((startParent == Adapter.GetFirstChild(startCell) || Adapter.GetChildren(startCell).TakeWhile(n => n != startParent).All(IsEmptyOrWhitespace))
								    && (startParent == Adapter.GetLastChild(startCell) || Adapter.GetChildren(startCell).SkipWhile(n => n != startParent).Skip(1).All(IsEmptyOrWhitespace)))
								{
									// Determine if the cell is the only cell in the row, or all other cells are empty
									if ((startCell == Adapter.GetFirstChild(startRow) || Adapter.GetChildren(startRow).TakeWhile(n => n != startCell).All(IsEmptyOrWhitespace))
									    && (startCell == Adapter.GetLastChild(startRow) || Adapter.GetChildren(startRow).SkipWhile(n => n != startCell).Skip(1).All(IsEmptyOrWhitespace)))
									{
										forceRemoveStartRow = true;
									}
								}
							}

							if (nodesToRemove.HasFlag(RegionNodes.End))
							{
								var beforeEnd = Adapter.GetPreviousSibling(region.EndToken.Start);

								// Determine if the end token is alone in its parent...
								if ((beforeEnd == null || (Adapter.GetChildren(endParent).TakeWhile(n => n != region.EndToken.Start).All(IsEmptyOrWhitespace)))
								    && (afterEnd == null || (Adapter.GetChildren(endParent).SkipWhile(n => n != region.EndToken.End).Skip(1).All(IsEmptyOrWhitespace))))
								{
									// Determine if the end token spans its entire cell...
									if ((endParent == Adapter.GetFirstChild(endCell) || Adapter.GetChildren(endCell).TakeWhile(n => n != endParent).All(IsEmptyOrWhitespace))
									    && (endParent == Adapter.GetLastChild(endCell) || Adapter.GetChildren(endCell).SkipWhile(n => n != endParent).Skip(1).All(IsEmptyOrWhitespace)))
									{
										// Determine if the cell is the only cell in the row, or all other cells are empty
										if ((endCell == Adapter.GetFirstChild(endRow) || Adapter.GetChildren(endRow).TakeWhile(n => n != endCell).All(IsEmptyOrWhitespace))
										    && (endCell == Adapter.GetLastChild(endRow) || Adapter.GetChildren(endRow).SkipWhile(n => n != endCell).Skip(1).All(IsEmptyOrWhitespace)))
										{
											forceRemoveEndRow = true;
										}
									}
								}
							}
						}

						Writer.RemoveRegionNodes(document, region.StartToken, region.EndToken, nodesToRemove, preserveTable: KeepEmptyRegionRows);

						var row = startRow;

						while (row != null)
						{
							var nextRow = Adapter.GetNextSibling(row);

							if (row == startRow)
								OnRowEmptied(row, forceRemoveStartRow);
							else if (row == endRow)
							{
								if (nodesToRemove.HasFlag(RegionNodes.End))
									OnRowEmptied(row, forceRemoveEndRow);

								break;
							}
							else
								OnRowEmptied(row);

							row = nextRow;
						}

						return;
					}
				}
			}

			base.RemoveRegion(document, region);
		}

		/// <summary>
		/// Generate nodes for the given value to replace the given standard field.
		/// </summary>
		protected override TNode[] GenerateStandardFieldContent(TDocument document, Field<TDocument, TNode, DocumentToken<TNode>, TSourceType, TSource, TExpression> field, string textValue)
		{
			TNode firstRun = null;

			for (TNode node = field.Token.Start; node != null; node = Adapter.GetNextSibling(node))
			{
				if (Adapter.GetNodeType(node) == DocumentNodeType.Run)
				{
					firstRun = node;
					break;
				}
			}

			TNode newRun;
			if (firstRun != null)
			{
				// If a run could be found, then clone it in order
				// to preserve the formatting of the original field.
				newRun = Adapter.Clone(firstRun, false);
				Adapter.SetText(newRun, textValue);
			}
			else
			{
				newRun = Adapter.CreateTextRun(document, textValue);
			}

			return new[] {newRun};
		}
	}
}
