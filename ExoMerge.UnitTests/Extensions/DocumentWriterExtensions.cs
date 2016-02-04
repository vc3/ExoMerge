using System;
using System.Linq;
using System.Text.RegularExpressions;
using ExoMerge.UnitTests.Common;

namespace ExoMerge.UnitTests.Extensions
{
	public static class DocumentWriterExtensions
	{
		public static void InsertMarkdown<TDocument, TNode, TCompositeNode, TRun>(this IDocumentWriter<TDocument, TNode, TCompositeNode, TRun> writer, string markdown)
			where TCompositeNode : TNode
			where TRun : TNode
		{
			var hasTable = false;
			var hasTableHeader = false;
			var expectTableSeparator = false;
			var expectTableContent = false;
			var hasTableContent = false;

			//var tableRowCount = 0;

			var tableCellCount = 0;

			var tableHeaderExpr = new Regex(@"^\|?(?:(?<name>(?<=\||^)\s*[^\s\|][^\|]*(?=\||$))\|?)+$");
			var tableSeparatorExpr = new Regex(@"^\|?(?:(?<separator>(?<=\||^)\s*\-\-\-\-*\s*(?=\||$))\|?)+$");
			var tableContentExpr = new Regex(@"^\|?(?:(?<content>(?<=\||^)[^\|][^\|]*(?=\||$))\|?)+$");

			var lines = markdown.Split(new[] { "\r\n" }, StringSplitOptions.None).Select(l => l.TrimStart()).ToArray();

			for (var i = 0; i < lines.Length; i++)
			{
				var line = lines[i];

				if (hasTable)
				{
					if (string.IsNullOrEmpty(line))
					{
						writer.EndTable();

						hasTable = false;
						hasTableHeader = false;
						expectTableContent = false;
						hasTableContent = false;
						tableCellCount = 0;
					}
					else if (expectTableSeparator)
					{
						if (!tableSeparatorExpr.IsMatch(line))
							throw new Exception("Invalid table with no header/content separator.");

						hasTableHeader = true;
						expectTableSeparator = false;
						expectTableContent = true;
					}
					else if (tableContentExpr.IsMatch(line))
					{
						writer.StartRow();

						var captures = tableContentExpr.Match(line).Groups["content"].Captures;

						var colspan = 1;

						if (captures.Count == 1 && tableCellCount > 1)
							colspan = tableCellCount;
						else if (captures.Count != tableCellCount)
							throw new Exception("Inconsistent number of cells in table, expected " + tableCellCount + ", found " + captures.Count + ".");

						foreach (Capture capture in captures)
						{
							writer.StartCell(colspan: colspan);

							var cellContents = capture.Value.Trim();

							cellContents = cellContents.Replace("&nbsp;", " ");

							if (cellContents.Contains("\\n"))
							{
								var cellParas = cellContents.Split(new[] {"\\n"}, StringSplitOptions.None);
								for (var j = 0; j < cellParas.Length; j++)
								{
									if (j == 0)
										writer.Write(cellParas[j]);
									else
										writer.WriteBlock(cellParas[j]);
								}
							}
							else
							{
								writer.Write(cellContents);
							}
						}

						writer.EndRow();

						hasTableContent = true;
					}
					else
						throw new Exception(string.Format("Invalid table line '{0}'.", line));
				}
				else if (line.IndexOf("|", StringComparison.Ordinal) >= 0 && tableHeaderExpr.IsMatch(line))
				{
					writer.StartTable();

					writer.StartRow();

					foreach (Capture capture in tableHeaderExpr.Match(line).Groups["name"].Captures)
					{
						writer.StartCell();
						tableCellCount++;

						writer.Write(capture.Value.Trim());

						writer.EndCell();
					}

					writer.EndRow();

					hasTable = true;
					hasTableHeader = true;
					expectTableSeparator = true;
				}
				else if (line.IndexOf("|", StringComparison.Ordinal) >= 0 && tableSeparatorExpr.IsMatch(line))
				{
					writer.StartTable();

					foreach (Capture capture in tableSeparatorExpr.Match(line).Groups["separator"].Captures)
					{
						tableCellCount++;
					}

					hasTable = true;
					hasTableHeader = false;
					expectTableSeparator = false;
				}
				else if (string.IsNullOrEmpty(line))
					writer.WriteBlock();
				else
				{
					var headingLevel = 0;

					line = line.Replace("&nbsp;", " ");

					if (line.StartsWith("######"))
					{
						headingLevel = 6;
						line = line.Substring(headingLevel).TrimStart();
					}
					else if (line.StartsWith("#####"))
					{
						headingLevel = 5;
						line = line.Substring(headingLevel).TrimStart();
					}
					else if (line.StartsWith("####"))
					{
						headingLevel = 4;
						line = line.Substring(headingLevel).TrimStart();
					}
					else if (line.StartsWith("###"))
					{
						headingLevel = 3;
						line = line.Substring(headingLevel).TrimStart();
					}
					else if (line.StartsWith("##"))
					{
						headingLevel = 2;
						line = line.Substring(headingLevel).TrimStart();
					}
					else if (line.StartsWith("#"))
					{
						headingLevel = 1;
						line = line.Substring(headingLevel).TrimStart();
					}
					else if (i < lines.Length - 1)
					{
						var nextLine = lines[i + 1];

						if (nextLine.StartsWith("==="))
						{
							headingLevel = 1;

							// Skip over the next line
							i++;
						}
						else if (nextLine.StartsWith("---"))
						{
							headingLevel = 2;

							// Skip over the next line
							i++;
						}
					}

					if (headingLevel > 0)
						writer.WriteHeading(line, headingLevel);
					else
						writer.WriteBlock(line);
				}
			}
		}
	}
}
