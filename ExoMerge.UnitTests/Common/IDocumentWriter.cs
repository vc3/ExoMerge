using System;

namespace ExoMerge.UnitTests.Common
{
	public interface IDocumentWriter<in TDocument, TNode, TCompositeNode, TRun>
		where TCompositeNode : TNode
		where TRun : TNode
	{
		/// <summary>
		/// Write the given text to the document.
		/// </summary>
		/// <param name="value"></param>
		void Write(string value);

		/// <summary>
		/// Write an empty block-level element.
		/// </summary>
		void WriteBlock();

		/// <summary>
		/// Write a block-level element with the given content.
		/// </summary>
		void WriteBlock(string text);

		/// <summary>
		/// Write a block-level element and then invoke the given action to continue building/writing.
		/// </summary>
		void WriteBlock(Action action);

		/// <summary>
		/// Writes a heading of the given level.
		/// </summary>
		void WriteHeading(string text, int level);

		/// <summary>
		/// Start building a table with the given options.
		/// </summary>
		void StartTable(string width = null, double? cellpadding = null, double? cellspacing = null, int? borderWidth = null, string borderColor = null, string bgcolor = null);

		/// <summary>
		/// Start building a row with the given options.
		/// </summary>
		/// <param name="bgcolor"></param>
		void StartRow(string bgcolor = null);

		/// <summary>
		/// Start building a cell with the given options.
		/// </summary>
		void StartCell(string valign = null, string width = null, string padding = null, int? colspan = null, string className = null, string border = null, string align = null);

		/// <summary>
		/// Finish building the current cell.
		/// </summary>
		void EndCell();

		/// <summary>
		/// Finish building the current row.
		/// </summary>
		void EndRow();

		/// <summary>
		/// Finish building the current table.
		/// </summary>
		void EndTable();
	}
}
