using System;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Run = DocumentFormat.OpenXml.Wordprocessing.Run;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;

namespace ExoMerge.OpenXml.UnitTests.Helpers
{
	internal class DocumentBuilder
	{
		private readonly WordprocessingDocument document;

		private readonly MainDocumentPart main;

		private readonly Body body;

		private Paragraph currentParagraph = null;

		private Table currentTable = null;

		public DocumentBuilder(WordprocessingDocument document, bool isEmptyDocument)
		{
			this.document = document;

			if (isEmptyDocument)
			{
				main = document.AddMainDocumentPart();
				main.Document = new Document();
				body = main.Document.AppendChild(new Body());

				InsertParagraph();
			}
			else
			{
				throw new Exception("Manipulating existing documents is not supported.");
			}
		}

		public WordprocessingDocument Document
		{
			get { return document; }
		}

		public Paragraph InsertParagraph()
		{
			var para = body.AppendChild(new Paragraph());
			currentParagraph = para;
			return para;
		}

		public Table StartTable()
		{
			var table = new Table();

			body.Append(table);

			currentParagraph = null;
			currentTable = table;

			return table;
		}

		public Cell StartCell()
		{
			throw new System.NotImplementedException();
		}

		public void EndTable()
		{
			throw new System.NotImplementedException();
		}

		public Run InsertRun(string trim)
		{
			throw new System.NotImplementedException();
		}
	}
}
