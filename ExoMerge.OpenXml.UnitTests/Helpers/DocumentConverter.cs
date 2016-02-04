using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ExoMerge.OpenXml.UnitTests.Helpers
{
	internal static class DocumentConverter
	{
		//public static Document FromMarkup(string markup)
		//{
		//	var builder = new DocumentBuilder();
		//	builder.InsertHtml(markup);
		//	return builder.Document;
		//}

		/*
		public static Document FromStrings(string[] strings)
		{
			Run[] runs;
			return FromStrings(strings, out runs);
		}

		public static Document FromStrings(string[] strings, out Run[] runs)
		{
			var builder = new DocumentBuilder();
			runs = strings.Select(builder.InsertRun).ToArray();
			return builder.Document;
		}
		*/

		public static WordprocessingDocument FromText(string text, string path = null, string internalName = "Template")
		{
			string internalPath;
			string resultPath;

			if (path != null)
			{
				resultPath = path;

				var directory = Path.GetDirectoryName(path);
				if (directory == null)
					throw new Exception("Invalid path: " + path);

				var fileName = Path.GetFileNameWithoutExtension(path);
				var extension = Path.GetExtension(path);

				internalPath = Path.Combine(directory, string.Format("{0}-{1}{2}", fileName, internalName, extension));
			}
			else
			{
				var id = Guid.NewGuid();
				internalPath = Path.Combine(Path.GetTempPath(), id + "-" + internalName + ".docx");
				resultPath = Path.Combine(Path.GetTempPath(), id + ".docx");
			}

			using (var document = WordprocessingDocument.Create(internalPath, WordprocessingDocumentType.Document))
			{
				var main = document.AddMainDocumentPart();
				main.Document = new Document();

				var body = main.Document.AppendChild(new Body());

				var currentParagraph = body.AppendChild(new Paragraph());

				Table currentTable = null;

				var isFirstBlock = true;

				var tableRowCount = 0;
				var tableCellCount = 0;

				var tableHeaderExpr = new Regex(@"^\|?(?:(?<name>(?<=\||^)\s*[^\s\|][^\|]*(?=\||$))\|?)+$");
				var tableSeparatorExpr = new Regex(@"^\|?(?:(?<separator>(?<=\||^)\s*\-\-\-\-*\s*(?=\||$))\|?)+$");
				var tableContentExpr = new Regex(@"^\|?(?:(?<content>(?<=\||^)\s*[^\s\|][^\|]*(?=\||$))\|?)+$");

				var lines = text.Split(new[] {"\r\n"}, StringSplitOptions.None);

				foreach (var line in lines.Select(l => l.TrimStart()))
				{
					if (tableRowCount > 0)
					{
						if (string.IsNullOrEmpty(line))
						{
							if (tableRowCount <= 2)
								throw new Exception("Invalid table with no data.");

							currentTable = null;
							currentParagraph = null;
							tableRowCount = 0;
							tableCellCount = 0;
						}
						else if (tableRowCount == 1)
						{
							if (!tableSeparatorExpr.IsMatch(line))
								throw new Exception("Invalid table with no header/content separator.");

							tableRowCount = 2;
						}
						else if (tableContentExpr.IsMatch(line))
						{
							var thisRowCellCount = 0;

							var row = currentTable.AppendChild(new TableRow());

							foreach (Capture capture in tableContentExpr.Match(line).Groups["content"].Captures)
							{
								var cell = row.AppendChild(new TableCell());

								thisRowCellCount++;

								var para = cell.AppendChild(new Paragraph());
								var run = para.AppendChild(new Run());

								run.AppendChild(new Text(capture.Value.Trim()));
							}

							if (thisRowCellCount != tableCellCount)
								throw new Exception("Inconsistent number of cells in table.");

							tableRowCount++;
						}
						else
							throw new Exception(string.Format("Invalid table line '{0}'.", line));
					}
					else if (line.IndexOf("|", StringComparison.Ordinal) >= 0 && tableHeaderExpr.IsMatch(line))
					{
						currentTable = document.MainDocumentPart.Document.Body.AppendChild(new Table());

						currentTable.AppendChild(new TableProperties(new TableWidth() {Width = "100%"}, new TableBorders(
							new TopBorder
							{
								Val = new EnumValue<BorderValues>(BorderValues.Single),
								Size = 8
							},
							new BottomBorder
							{
								Val = new EnumValue<BorderValues>(BorderValues.Single),
								Size = 8
							},
							new LeftBorder
							{
								Val = new EnumValue<BorderValues>(BorderValues.Single),
								Size = 8
							},
							new RightBorder
							{
								Val = new EnumValue<BorderValues>(BorderValues.Single),
								Size = 8
							},
							new InsideHorizontalBorder
							{
								Val = new EnumValue<BorderValues>(BorderValues.Single),
								Size = 8
							},
							new InsideVerticalBorder
							{
								Val = new EnumValue<BorderValues>(BorderValues.Single),
								Size = 8
							})));

						var row = currentTable.AppendChild(new TableRow());

						foreach (Capture capture in tableHeaderExpr.Match(line).Groups["name"].Captures)
						{
							var cell = row.AppendChild(new TableCell());

							tableCellCount++;

							var para = cell.AppendChild(new Paragraph());
							var run = para.AppendChild(new Run());

							run.AppendChild(new Text(capture.Value.Trim()));
						}

						tableRowCount = 1;
					}
					else
					{
						if (!isFirstBlock)
							currentParagraph = body.AppendChild(new Paragraph());

						if (!string.IsNullOrEmpty(line))
						{
							var run = currentParagraph.AppendChild(new Run());

							run.AppendChild(new Text(line));
						}

						isFirstBlock = false;
					}
				}

				main.Document.Save();
			}

			File.Copy(internalPath, resultPath);

			return WordprocessingDocument.Open(resultPath, true);
		}
	}
}
