using System;
using System.Collections.Generic;
using System.Drawing;
using Aspose.Words;
using Aspose.Words.Tables;
using ExoMerge.UnitTests.Common;

namespace ExoMerge.Aspose.UnitTests.Helpers
{
	public class DocumentTemplateWriter : IDocumentWriter<Document, Node, CompositeNode, Run>
	{
		private Table currentTable;

		private readonly Stack<Table> ancestorTables = new Stack<Table>();

		private Row currentRow;

		private readonly Stack<Row> ancestorRows = new Stack<Row>();

		private Cell currentCell;

		private readonly Stack<Cell> ancestorCells = new Stack<Cell>();

		private bool isRowPending;

		private readonly Dictionary<Table, Tuple<int, string>> tableBorders = new Dictionary<Table, Tuple<int, string>>();

		private readonly Dictionary<Table, double> tableWidths = new Dictionary<Table, double>();

		private readonly Dictionary<Table, double> tableCellPadding = new Dictionary<Table, double>();

		private readonly Dictionary<Table, double> tableCellSpacing = new Dictionary<Table, double>();

		private bool hasEmptyBlock;

		public DocumentTemplateWriter()
		{
			Builder = new DocumentBuilder();
			hasEmptyBlock = true;
		}

		public DocumentTemplateWriter(Document document)
		{
			Builder = new DocumentBuilder(document);
		}

		public DocumentBuilder Builder { get; private set; }

		public Document Document
		{
			get { return Builder.Document; }
		}

		public void Write(string value)
		{
			Builder.Write(value);
		}

		public void WriteBlock()
		{
			if (hasEmptyBlock)
				hasEmptyBlock = false;
			else
				Builder.InsertParagraph();
		}

		public void WriteBlock(string text)
		{
			WriteBlock();
			Builder.Write(text);
		}

		public void WriteBlock(Action action)
		{
			WriteBlock();
			action();
		}

		public void WriteHeading(string text, int level)
		{
			WriteBlock();

			if (level == 1)
				Builder.CurrentParagraph.ParagraphFormat.StyleName = "Heading 1";
			else if (level == 2)
				Builder.CurrentParagraph.ParagraphFormat.StyleName = "Heading 2";
			else
				throw new Exception("Heading level " + level + " is not supported.");

			Builder.Write(text);
		}

		public void StartTable(string width = null, double? cellpadding = null, double? cellspacing = null, int? borderWidth = null, string borderColor = null, string bgcolor = null)
		{
			if (hasEmptyBlock)
				hasEmptyBlock = false;

			if (isRowPending)
				throw new InvalidOperationException();

			var table = Builder.StartTable();

			if (width != null)
			{
				double tableWidth;

				if (width.EndsWith("%"))
					tableWidth = double.Parse(width.Substring(0, width.Length - 1));
				else
					throw new InvalidOperationException();

				tableWidths.Add(table, tableWidth);
			}

			if (cellpadding != null)
				tableCellPadding.Add(table, cellpadding.Value);

			if (cellspacing != null)
				tableCellSpacing.Add(table, cellspacing.Value);

			if (borderWidth != null)
				tableBorders.Add(table, new Tuple<int, string>(borderWidth.Value, borderColor));

			if (currentCell != null)
			{
				ancestorCells.Push(currentCell);
				ancestorRows.Push(currentRow);
				ancestorTables.Push(currentTable);
			}
			else if (currentTable != null)
				throw new InvalidOperationException();

			currentTable = table;
			currentRow = null;
			currentCell = null;
		}

		public void StartRow(string bgcolor = null)
		{
			if (isRowPending)
				throw new InvalidOperationException();

			if (currentCell != null)
			{
				ancestorCells.Push(currentCell);
				ancestorRows.Push(currentRow);
			}

			currentCell = null;
			currentRow = null;
			isRowPending = true;
		}

		private static void ApplyCellStyles(Cell cell, IDictionary<Table, double> tableCellPaddings, IReadOnlyDictionary<Table, double> tableCellSpacings)
		{
			var table = cell.ParentRow.ParentTable;

			double cellpadding;
			if (tableCellPaddings.TryGetValue(table, out cellpadding))
			{
				cell.CellFormat.TopPadding = cellpadding;
				cell.CellFormat.RightPadding = cellpadding;
				cell.CellFormat.BottomPadding = cellpadding;
				cell.CellFormat.LeftPadding = cellpadding;
			}
		}

		private double GetSizeValue(string value)
		{
			if (value == "0")
				return 0;

			if (value.EndsWith("px"))
				return int.Parse(value.Substring(0, value.Length - 2));

			throw new InvalidOperationException();
		}

		public void StartCell(string valign = null, string width = null, string padding = null, int? colspan = null, string className = null, string border = null, string align = null)
		{
			var cell = Builder.InsertCell();

			ApplyCellStyles(cell, tableCellPadding, tableCellSpacing);

			if (valign != null)
			{
				var valignEnum = (CellVerticalAlignment)Enum.Parse(typeof(CellVerticalAlignment), valign, true);
				cell.CellFormat.VerticalAlignment = valignEnum;
			}

			if (padding != null)
			{
				if (padding == "0")
				{
					cell.CellFormat.TopPadding = 0;
					cell.CellFormat.RightPadding = 0;
					cell.CellFormat.BottomPadding = 0;
					cell.CellFormat.LeftPadding = 0;
				}
				else
				{
					var paddingParts = padding.Split(' ');
					if (paddingParts.Length != 4)
						throw new InvalidOperationException();

					cell.CellFormat.TopPadding = GetSizeValue(paddingParts[0]);
					cell.CellFormat.RightPadding = GetSizeValue(paddingParts[1]);
					cell.CellFormat.BottomPadding = GetSizeValue(paddingParts[2]);
					cell.CellFormat.LeftPadding = GetSizeValue(paddingParts[3]);
				}
			}

			if (width != null)
			{
				if (width.EndsWith("%"))
				{
					var widthPercentage = int.Parse(width.Substring(0, width.Length - 1));
					var widthFraction = (double)widthPercentage / 100;
					cell.CellFormat.Width = Builder.Document.GetPageInfo(0).WidthInPoints * widthFraction;
					cell.CellFormat.PreferredWidth = PreferredWidth.FromPercent(widthPercentage);
				}
				else
					throw new InvalidOperationException();
			}
			else
				cell.CellFormat.PreferredWidth = PreferredWidth.Auto;

			if (className == null)
				Builder.CurrentParagraph.ParagraphFormat.StyleName = "Body Text";
			else
			{
				switch (className)
				{
					case "helptext":
						Builder.CurrentParagraph.ParagraphFormat.StyleName = "Help Text";
						break;
					case "c-label":
						Builder.CurrentParagraph.ParagraphFormat.StyleName = "Label Text";
						break;
					case "c-forms-heading":
						break;
					default:
						throw new InvalidOperationException();
				}
			}

			if (currentRow == null)
			{
				if (!isRowPending)
					throw new InvalidOperationException("A row has not been started.");

				currentRow = cell.ParentRow;
				isRowPending = false;
			}


			currentCell = cell;
		}

		public void EndCell()
		{
			currentCell = null;
		}

		public void EndRow()
		{
			if (isRowPending)
				throw new InvalidOperationException();

			Builder.EndRow();

			if (ancestorCells.Count > 0 && ancestorCells.Peek().ParentRow.ParentTable == currentTable)
			{
				currentCell = ancestorCells.Pop();
				currentRow = ancestorRows.Pop();
			}
			else
			{
				currentCell = null;
				currentRow = null;
			}

			isRowPending = false;
		}

		public void EndTable()
		{
			if (currentTable == null)
				throw new InvalidOperationException();

			double cellpadding;
			if (tableCellPadding.TryGetValue(currentTable, out cellpadding))
			{
				currentTable.TopPadding = cellpadding;
				currentTable.RightPadding = cellpadding;
				currentTable.BottomPadding = cellpadding;
				currentTable.LeftPadding = cellpadding;
			}

			double cellspacing;
			if (tableCellSpacing.TryGetValue(currentTable, out cellspacing))
			{
				currentTable.CellSpacing = cellspacing;
			}

			Tuple<int, string> border;
			if (tableBorders.TryGetValue(currentTable, out border))
			{
				if (border.Item1 == 0)
					currentTable.ClearBorders();
				else
				{
					var borderColor = Color.Black;

					KnownColor knownColor;
					if (Enum.TryParse(border.Item2, true, out knownColor))
						borderColor = Color.FromKnownColor(knownColor);

					currentTable.SetBorders(LineStyle.Single, border.Item1, borderColor);
				}

				tableBorders.Remove(currentTable);
			}

			double tableWidth;

			if (tableWidths.TryGetValue(currentTable, out tableWidth))
			{
				currentTable.PreferredWidth = PreferredWidth.FromPercent(tableWidth);

				// http://www.aspose.com/docs/display/wordsnet/Specifying+Table+and+Cell+Widths
				// "The Table.AllowAutoFit property enables cells in the table to grow and shrink to
				// accommodate their contents. This property can be used in conjunction with a
				// preferred cell width to format a cell which auto fits its content but which also has
				// an initial width. The cell width can then grow past this width if needed. Therefore
				// if you are using a table as layout or placeholder for content then you should
				// disable this property on your table."
				currentTable.AllowAutoFit = false;
			}
			else
			{
				currentTable.PreferredWidth = PreferredWidth.Auto;
			}

			if (ancestorTables.Count > 0)
			{
				currentTable = ancestorTables.Pop();
				currentRow = ancestorRows.Pop();
				currentCell = ancestorCells.Pop();
			}
			else
			{
				currentTable = null;
				currentRow = null;
				currentCell = null;
			}

			Builder.EndTable();
		}
	}
}
