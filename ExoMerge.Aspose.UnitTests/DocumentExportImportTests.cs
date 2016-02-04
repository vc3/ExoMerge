using System.Linq;
using Aspose.Words;
using AsposeSerializer;
using AsposeSerializer.Extensions;
using ExoMerge.Aspose.UnitTests.Common;
using ExoMerge.Aspose.UnitTests.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.Aspose.UnitTests
{
	[TestClass]
	public class DocumentExportImportTests : DocumentTestsBase
	{
		//[TestMethod]
		public void ExportXml_FromFileSystem()
		{
			var xml = new Document(@"C:\Temp\XmlInput.docx").Export(null, DocumentTextFormat.Xml);
			System.IO.File.WriteAllText(@"C:\Temp\XmlOutput.docx.xml", xml);
		}

		[TestMethod]
		public void ExportXml_EmptyDocument()
		{
			var doc = new Document();

			doc.AssertXml(@"
				<Document>
				  <Section>
					<Body>
					  <Paragraph>
					  </Paragraph>
					</Body>
				  </Section>
				</Document>");
		}

		[TestMethod]
		public void ExportXml_SimpleText()
		{
			var doc = new Document();

			var builder = new DocumentBuilder(doc);

			builder.Write("Hello World");

			doc.AssertXml(@"
				<Document>
				  <Section>
					<Body>
					  <Paragraph>
					    <Run>Hello World</Run>
					  </Paragraph>
					</Body>
				  </Section>
				</Document>");
		}

		[TestMethod]
		public void ExportImportXml_SimpleText()
		{
			var exportDoc = new Document();

			var exportBuilder = new DocumentBuilder(exportDoc);

			exportBuilder.Write("Hello World");

			var exportBodyXml = exportDoc.Export(exportDoc.Sections.Cast<Section>().Single().Body, DocumentTextFormat.Xml);

			var importDoc = new Document();

			importDoc.Import(importDoc.Sections.Cast<Section>().Single().Body, DocumentTextFormat.Xml, exportBodyXml);

			importDoc.AssertXml(importDoc.Sections.Cast<Section>().Single().Body, exportBodyXml);
		}

		[TestMethod]
		public void ImportExportXml_Table()
		{
			var doc = new Document();

			var importBodyXml = @"
				<Body>
				  <Table>
				    <Row>
				      <Cell>
				        <Paragraph>
				          <Run>Number</Run>
				        </Paragraph>
				      </Cell>
				      <Cell>
				        <Paragraph>
				          <Run>Name</Run>
				        </Paragraph>
				      </Cell>
				      <Cell>
				        <Paragraph>
				          <Run>Yes/No</Run>
				        </Paragraph>
				      </Cell>
				    </Row>
				    <Row>
				      <Cell>
				        <Paragraph>
				          <Run>1</Run>
				        </Paragraph>
				      </Cell>
				      <Cell>
				        <Paragraph>
				          <Run>J. Doe</Run>
				        </Paragraph>
				      </Cell>
				      <Cell>
				        <Paragraph>
				          <Run>Yes</Run>
				        </Paragraph>
				      </Cell>
				    </Row>
				    <Row>
				      <Cell>
				        <Paragraph>
				          <Run>2</Run>
				        </Paragraph>
				      </Cell>
				      <Cell>
				        <Paragraph>
				          <Run>A. Smith</Run>
				        </Paragraph>
				      </Cell>
				      <Cell>
				        <Paragraph>
				          <Run>No</Run>
				        </Paragraph>
				      </Cell>
				    </Row>
				  </Table>
				</Body>
				";

			doc.Import(doc.Sections.Cast<Section>().Single().Body, DocumentTextFormat.Xml, importBodyXml);

			doc.AssertXml(doc.Sections.Cast<Section>().Single().Body, importBodyXml);
		}
	}
}
