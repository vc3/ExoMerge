using System;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using ExoMerge.OpenXml.UnitTests.Helpers;
using ExoMerge.UnitTests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.OpenXml.UnitTests
{
	[TestClass]
	public class DocumentRenderTests : TestsBase
	{
		[TestMethod]
		public void CreateOpenXmlDocument()
		{
			using (var memoryStream = new MemoryStream())
			{
				using (var doc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document))
				{
					var main = doc.AddMainDocumentPart();

					main.Document = new Document();

					var body = main.Document.AppendChild(new Body());
					var para = body.AppendChild(new Paragraph());
					var run = para.AppendChild(new Run());
					run.AppendChild(new Text("Hello world!"));

					main.Document.Save();
				}

				memoryStream.Seek(0, SeekOrigin.Begin);

				var path = Path.Combine(Path.GetTempPath(), "OpenXml-" + Guid.NewGuid() + ".docx");
				using (var fileStream = File.OpenWrite(path))
				{
					memoryStream.CopyTo(fileStream);
				}
			}
		}

		[TestMethod]
		public void MergeOpenXml_ListInTable_RowIsRepeatedForEachItem()
		{
			var path = Path.Combine(Path.GetTempPath(), "OpenXml-" + Guid.NewGuid() + ".docx");

			const string documentText = @"

				| Item                       | In Stock      | Price                     |
				| -------------------------- | ------------- | ------------------------- |
				| {{ each Items }}{{ Name }} | {{ InStock }} | {{ Price }}{{ end each }} |

				";

			using (var document = DocumentConverter.FromText(documentText, path))
			{
				var mergeProvider = new DocumentMergeProvider<Type, object, string>("{{", "}}", new SimpleTokenParser(), new SimpleExpressionParser(), null, new SimpleDataProvider());

				var dataSource = new
				{
					Items = new[]
						{
							new {Name = "Milk", InStock = true, Price = 3.50},
							new {Name = "Eggs", InStock = true, Price = 2.25},
							new {Name = "Artisan Bread", InStock = false, Price = 6.99},
						},
				};

				mergeProvider.Merge(document, dataSource);

				document.MainDocumentPart.Document.Save();
			}
		}
	}
}
