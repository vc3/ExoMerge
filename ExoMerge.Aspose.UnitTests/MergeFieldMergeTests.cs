using ExoMerge.Aspose.UnitTests.Common;
using ExoMerge.Aspose.UnitTests.Extensions;
using ExoMerge.Aspose.UnitTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.Aspose.UnitTests
{
	[TestClass]
	public class MergeFieldMergeTests : DocumentTestsBase
	{
		[TestMethod]
		public void MergeDocFields_RootMergeField_NodesReplacedWithValue()
		{
			var doc = DocumentConverter.FromDisplayCode(@"{ MERGEFIELD FullName }");

			var data = new
			{
				FullName = "George Washington"
			};

			doc.Merge(data, true);

			AssertText(doc, "George Washington\r\n");
		}

		[TestMethod]
		public void MergeDocFields_ListMergeField_MergeRepeatedPerItem()
		{
			var doc = DocumentConverter.FromDisplayCode(@"{ MERGEFIELD List:Letters }{ MERGEFIELD Text }{ MERGEFIELD EndList:Letters }");

			var data = new
			{
				Letters = new[]
				{
					new {Text = "A"},
					new {Text = "B"},
					new {Text = "C"}
				}
			};

			doc.Merge(data, true);

			AssertText(doc, @"ABC
							");
		}

		[TestMethod]
		public void MergeDocFields_ListMergeFieldThatSpanParagraphs_MergeRepeatedPerItem()
		{
			var doc = DocumentConverter.FromDisplayCode(@"
				{ MERGEFIELD List:Items }
				{ MERGEFIELD Name }
				{ MERGEFIELD EndList:Items }
				");

			var data = new
			{
				Items = new[] {
						new {Name = "Inkjet Printer"},
						new {Name = "Stapler"},
						new {Name = "Paperclips"}
					}
			};

			doc.Merge(data, true);

			AssertText(doc, @"Inkjet Printer
							Stapler
							Paperclips
							");
		}

		[TestMethod]
		public void MergeDocFields_ListMergeFieldThatSpanParagraphsNonStandard_MergeRepeatedPerItem()
		{
			var doc = DocumentConverter.FromDisplayCode(@"
				^{ MERGEFIELD List:BaseballTeamsInChicago }
				{ MERGEFIELD Name }{ MERGEFIELD EndList:BaseballTeamsInChicago }$
				");

			var data = new
			{
				BaseballTeamsInChicago = new[] {
						new {Name = "White Sox", Sequence=2 },
						new {Name = "Cubs", Sequence=1 }
					}
			};

			doc.Merge(data, true);

			AssertText(doc, @"^
							White Sox
							Cubs$
							");
		}

		[TestMethod]
		public void MergeDocFields_IfMergeFieldHasValue_BlockIsRendered()
		{
			var doc = DocumentConverter.FromDisplayCode(@"{ MERGEFIELD If:Bcc }BCC: { MERGEFIELD Bcc }{ MERGEFIELD EndIf:Bcc }");

			var data = new
			{
				Bcc = "It's a Secret",
			};

			doc.Merge(data, true);

			AssertText(doc, @"BCC: It's a Secret
			               ");
		}

		[TestMethod]
		public void MergeDocFields_IfMergeFieldFalseValue_BlockIsRendered()
		{
			var doc = DocumentConverter.FromDisplayCode(@"{ MERGEFIELD If:ShouldSendNotice }To: { MERGEFIELD Recipient }{ MERGEFIELD EndIf:ShouldSendNotice }");

			var data = new
			{
				Recipient = "Jon Doe",
				ShouldSendNotice = false,
			};

			doc.Merge(data, true);

			AssertText(doc, @"");
		}
	}
}
