using System;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ExoMerge.OpenXml.Extensions
{
	public static class OpenXmlElementExtensions
	{
		public static TElement Clone<TElement>(this TElement element, bool recursive)
			where TElement : OpenXmlElement
		{
			if (recursive)
				throw new NotImplementedException("Recursive cloning is not implemented.");

			var run = element as Run;
			if (run != null)
				return Clone(run).Cast<TElement>();

			var row = element as TableRow;
			if (row != null)
				return Clone(row).Cast<TElement>();

			var cell = element as TableCell;
			if (cell != null)
				return Clone(cell).Cast<TElement>();

			var para = element as Paragraph;
			if (para != null)
				return Clone(para).Cast<TElement>();

			throw new Exception("Unable to clone element of type '" + element.GetType().Name + "'.");
		}

		public static TElement Cast<TElement>(this OpenXmlElement element)
			where TElement : OpenXmlElement
		{
			var result = element as TElement;
			if (result == null)
				throw new InvalidCastException("Unable to cast element of type '" + element.GetType().Name + "' to type '" + typeof(TElement).Name + "'.");
			return result;
		}

		private static Run Clone(Run run)
		{
			var newRun = new Run();

			foreach (var child in run.ChildElements)
			{
				var text = child as Text;
				if (text == null)
					throw new Exception("Invalid child element of type '" + child.GetType().Name + "'.");

				newRun.AppendChild(new Text(text.Text));
			}

			return newRun;
		}

		private static Paragraph Clone(Paragraph para)
		{
			var newPara = new Paragraph();

			return newPara;
		}

		private static TableRow Clone(TableRow row)
		{
			var newRow = new TableRow();

			return newRow;
		}

		private static TableCell Clone(TableCell cell)
		{
			var newCell = new TableCell();

			return newCell;
		}
	}
}
