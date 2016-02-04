using System;

namespace ExoMerge.Documents
{
	[Flags]
	public enum DocumentNodeType
	{
		/// <summary>
		/// A node of unknown type
		/// </summary>
		Unknown,

		/// <summary>
		/// An inline run of text
		/// </summary>
		Run,

		/// <summary>
		/// A block-level paragraph of text
		/// </summary>
		Paragraph,

		/// <summary>
		/// A table
		/// </summary>
		Table,

		/// <summary>
		/// A table row
		/// </summary>
		TableRow,

		/// <summary>
		/// A table cell
		/// </summary>
		TableCell,
	}
}