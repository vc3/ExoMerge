using System;
using Aspose.Words;
using ExoMerge.Analysis;
using ExoMerge.Aspose.Common;
using ExoMerge.Aspose.UnitTests.Common;
using ExoMerge.Aspose.UnitTests.Extensions;
using ExoMerge.Aspose.UnitTests.Helpers;
using ExoMerge.Rendering;
using ExoMerge.UnitTests.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.Aspose.UnitTests
{
	[TestClass]
	public class NodeTextMergeTests : DocumentTestsBase
	{
		[TestMethod]
		public void MergeDocText_RootField_NodesReplacedWithValue()
		{
			var doc = DocumentConverter.FromStrings(new[] { "{{ Business }}" });

			var data = new
			{
				Business = "Dave's Automotive",
			};

			doc.Merge(data);

			AssertText(doc, @"Dave's Automotive
							");
		}

		[TestMethod]
		public void MergeDocText_EachRegion_MergeRepeatedPerItem()
		{
			var doc = DocumentConverter.FromStrings(new[] { @"{{ each States }}", "{{ Name }},", "{{ end each }}" });

			var data = new
			{
				States = new[] {
						new {Name = "Wyoming" },
						new {Name = "Arkansas" },
						new {Name = "Vermont" }
					},
			};

			doc.Merge(data);

			AssertText(doc, @"Wyoming,Arkansas,Vermont,
							");
		}

		[TestMethod]
		public void MergeDocText_ListThatSpanParagraphs_MergeRepeatedPerItem()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"^
				{{ each SportsLeagues }}
				{{ Acronym }}
				{{ end each }}
				$");

			var data = new
			{
				SportsLeagues = new[] {
						new {Acronym = "MLB" },
						new {Acronym = "NBA" },
						new {Acronym = "NFL" },
						new {Acronym = "NHL" },
						new {Acronym = "MLS" },
					},
			};

			writer.Document.Merge(data);

			AssertText(writer.Document, @"^
							MLB
							NBA
							NFL
							NHL
							MLS
							$
							");
		}

		[TestMethod]
		public void MergeDocText_ListWithEmptyParagraphs_EmptyParagraphsPreserved()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"^
				{{ each CardinalDirections }}

				{{ Symbol }}

				{{ Name }}
				{{ end each }}

				$");

			var data = new
			{
				CardinalDirections = new[] {
						new {Name = "North", Symbol='↑' },
						new {Name = "South", Symbol='↓' },
						new {Name = "East", Symbol='→' },
						new {Name = "West", Symbol='←' },
					},
			};

			writer.Document.Merge(data);

			AssertText(writer.Document, @"^

							↑

							North

							↓

							South

							→

							East

							←

							West

							$
							");
		}

		[TestMethod]
		public void MergeDocText_ListThatSpanParagraphsNonStandard_MergeRepeatedPerItem()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"^{{ each TimeZones }}
				{{ Name }}{{ end each }}$");

			var data = new
			{
				TimeZones = new[] {
						new {Name = "Eastern" },
						new {Name = "Central" },
						new {Name = "Mountain West" },
						new {Name = "Pacific" },
					},
			};

			writer.Document.Merge(data);

			AssertText(writer.Document, @"^
							Eastern
							Central
							Mountain West
							Pacific$
							");
		}

		[TestMethod]
		public void MergeDocText_ListWithErrors_MergeRepeatedPerItem()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"^{{ each TimeZones }}
				{{ Name }} - {{ $foo }}{{ end each }}$");

			var data = new
			{
				TimeZones = new[] {
						new {Name = "Eastern" },
						new {Name = "Central" },
						new {Name = "Mountain West" },
						new {Name = "Pacific" },
					},
			};

			IMergeError[] errors;

			writer.Document.TryMerge(data, out errors);

			AssertText(writer.Document, @"^
							Eastern - {{ $foo }}
							Central - {{ $foo }}
							Mountain West - {{ $foo }}
							Pacific - {{ $foo }}$
							");
		}

		[TestMethod]
		public void MergeDocText_IfHasValue_BlockIsRendered()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"{{ if Recipients }}To: {{ each Recipients }}{{ Name }}, {{ end each }} Subject: {{ Title }}{{ end if }}");

			var data = new
			{
				Title = "Information About Your Policy",
				Recipients = new[] {
						new {Name = "Jane Doe" },
						new {Name = "Jon Doe" },
					},
			};

			writer.Document.Merge(data);

			AssertText(writer.Document, @"To: Jane Doe, Jon Doe,  Subject: Information About Your Policy
			               ");
		}

		[TestMethod]
		public void MergeDocText_IfFalseValue_BlockIsNotRendered()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"{{ if ShouldSendOffer }}To: {{ Recipient }}{{ end if }}");

			var data = new
			{
				Title = "Free Vacation Offer",
				ShouldSendOffer = false,
				Recipient = "Valued Customer",
			};

			writer.Document.Merge(data);

			AssertText(writer.Document, @"");
		}

		[TestMethod]
		public void MergeDocText_IfFalseValue_MultipleBlocksAreNotRendered()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"^
									{{ if ShouldSendOffer }}
									To: {{ Recipient }}
									{{ end if }}
									$");

			var data = new
			{
				Title = "Free Vacation Offer",
				ShouldSendOffer = false,
				Recipient = "Valued Customer",
			};

			writer.Document.Merge(data);

			AssertText(writer.Document, @"^
							$
							");
		}

		[TestMethod]
		public void MergeDocText_IfWithMultipleOptions_SatsifyingBlockIsRendered()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"^
									{{ Title }}
									{{ if ShouldSendOffer }}
									To: {{ Recipient }}
									{{ else if IsUnderReview }}
									Status: <UNDER_REVIEW>
									{{ else }}
									Status: <UNKNOWN>
									{{ end if }}
									$");

			var data = new
			{
				Title = "Free Vacation Offer",
				IsUnderReview = true,
				ShouldSendOffer = false,
			};

			writer.Document.Merge(data);

			AssertText(writer.Document, @"^
							Free Vacation Offer
							Status: <UNDER_REVIEW>
							$
							");
		}

		[TestMethod]
		public void MergeDocText_IfWithMultipleOptions_NonSatisfyingDefaultBlockIsRendered()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"^
									{{ Title }}
									{{ if ShouldSendOffer }}
									To: {{ Recipient }}
									{{ else if IsUnderReview }}
									Status: <UNDER_REVIEW>
									{{ else }}
									Status: <UNKNOWN>
									{{ end if }}
									$");

			var data = new
			{
				Title = "Free Vacation Offer",
				IsUnderReview = false,
				ShouldSendOffer = false,
			};

			writer.Document.Merge(data);

			AssertText(writer.Document, @"^
							Free Vacation Offer
							Status: <UNKNOWN>
							$
							");
		}

		[TestMethod]
		public void MergeDocText_IfWithinList_BlockIsRenderedWhenConditionPasses()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"{{ each NcaaConferences }}{{ if Active }}{{ Name }}, {{ end if }}{{ end each }}");

			var data = new
			{
				NcaaConferences = new[] {
						new {Name = "PAC 12", Active = true },
						new {Name = "SEC", Active = true },
						new {Name = "Big East", Active = false }
					},
			};

			writer.Document.Merge(data);

			AssertText(writer.Document, @"PAC 12, SEC, 
			               ");
		}

		[TestMethod]
		public void MergeDocText_ListInTable_RowIsRepeatedForEachItem()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"

				| Item                       | In Stock      | Price                     |
				| -------------------------- | ------------- | ------------------------- |
				| {{ each Items }}{{ Name }} | {{ InStock }} | {{ Price }}{{ end each }} |

				");

			var data = new
			{
				Items = new[] {
						new {Name = "Milk", InStock = true, Price = 3.50 },
						new {Name = "Eggs", InStock = true, Price = 2.25 },
						new {Name = "Artisan Bread", InStock = false, Price = 6.99 }
					},
			};

			writer.Document.Merge(data);

			AssertText(writer.Document, @"
				Item
				In Stock
				Price
				Milk
				True
				3.5
				Eggs
				True
				2.25
				Artisan Bread
				False
				6.99


				");

			Assert.AreEqual(4, writer.Document.SelectNodes("//Row").Count, "The row should be repeated for each item.");
			Assert.AreEqual(12, writer.Document.SelectNodes("//Cell").Count, "The cells should be repeated in each row.");
		}

		[TestMethod]
		public void MergeDocText_ListInTable_RowIsRemovedIfNoItems()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"

				| Item                       | In Stock      | Price                     |
				| -------------------------- | ------------- | ------------------------- |
				| {{ each Items }}{{ Name }} | {{ InStock }} | {{ Price }}{{ end each }} |

				");

			var data = new
			{
				Items = new object[0],
			};

			writer.Document.Merge(data);

			writer.Document.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				      </Paragraph>
				      <Table preferredwidth='Auto'>
				        <Row rowformat_preferredwidth='Auto'>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>Item</Run>
				            </Paragraph>
				          </Cell>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>In Stock</Run>
				            </Paragraph>
				          </Cell>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>Price</Run>
				            </Paragraph>
				          </Cell>
				        </Row>
				      </Table>
				      <Paragraph>
				      </Paragraph>
				      <Paragraph>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");

			AssertText(writer.Document, @"

				| Item                       | In Stock      | Price                     |
				| -------------------------- | ------------- | ------------------------- |

				", asMarkdown: true);

			Assert.AreEqual(1, writer.Document.SelectNodes("//Row").Count, "Only the header row should remain.");
			Assert.AreEqual(3, writer.Document.SelectNodes("//Cell").Count, "Only the header cells should remain.");
		}

		[TestMethod]
		public void MergeDocText_ListInTable_RowIsRemovedIfTagOnly()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"

				| Item                      | In Stock      | Price          |
				| ------------------------- | ------------- | -------------- |
				| {{ each Items }}                                           |
				| {{ if Group }}{{ Group }}                                  |
				| {{ else }}None                                             |
				| {{ end if }}                                               |
				| {{ Name }}                | {{ InStock }} | {{ Price }}    |
				| {{ end each }}                                             |

				");

			var data = new
			{
				Items = new[] {
						new {Group = "Dairy", Name = "Milk", InStock = true, Price = 3.50 },
						new {Group = "Dairy", Name = "Eggs", InStock = true, Price = 2.25 },
						new {Group = "", Name = "Artisan Bread", InStock = false, Price = 6.99 }
					},
			};

			writer.Document.Merge(data);

			writer.Document.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				      </Paragraph>
				      <Table preferredwidth='Auto'>
				        <Row rowformat_preferredwidth='Auto'>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>Item</Run>
				            </Paragraph>
				          </Cell>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>In Stock</Run>
				            </Paragraph>
				          </Cell>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>Price</Run>
				            </Paragraph>
				          </Cell>
				        </Row>
				        <Row rowformat_preferredwidth='Auto'>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>Dairy</Run>
				            </Paragraph>
				          </Cell>
				        </Row>
				        <Row rowformat_preferredwidth='Auto'>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>Milk</Run>
				            </Paragraph>
				          </Cell>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>True</Run>
				            </Paragraph>
				          </Cell>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>3.5</Run>
				            </Paragraph>
				          </Cell>
				        </Row>
				        <Row rowformat_preferredwidth='Auto'>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>Dairy</Run>
				            </Paragraph>
				          </Cell>
				        </Row>
				        <Row rowformat_preferredwidth='Auto'>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>Eggs</Run>
				            </Paragraph>
				          </Cell>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>True</Run>
				            </Paragraph>
				          </Cell>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>2.25</Run>
				            </Paragraph>
				          </Cell>
				        </Row>
				        <Row rowformat_preferredwidth='Auto'>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>None</Run>
				            </Paragraph>
				          </Cell>
				        </Row>
				        <Row rowformat_preferredwidth='Auto'>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>Artisan Bread</Run>
				            </Paragraph>
				          </Cell>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>False</Run>
				            </Paragraph>
				          </Cell>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>6.99</Run>
				            </Paragraph>
				          </Cell>
				        </Row>
				      </Table>
				      <Paragraph>
				      </Paragraph>
				      <Paragraph>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");
		}

		[TestMethod]
		public void MergeDocText_ListInTableWithBlankValues_RowRemains()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"

				| Item                       | In Stock      | Price                     |
				| -------------------------- | ------------- | ------------------------- |
				| {{ each Items }}{{ Name }} | {{ InStock }} | {{ Price }}{{ end each }} |

				");

			var data = new
			{
				Items = new[]
				{
					new {Name = "", InStock = "", Price = ""},
				},
			};

			writer.Document.Merge(data);

			AssertText(writer.Document, @"

				| Item   | In Stock | Price  |
				| ----   | -------- | -----  |
				| &nbsp; | &nbsp;   | &nbsp; |
				", asMarkdown: true);

			Assert.AreEqual(2, writer.Document.SelectNodes("//Row").Count, "The row should be repeated for each item.");
			Assert.AreEqual(6, writer.Document.SelectNodes("//Cell").Count, "The cells should be repeated in each row.");
		}

		[TestMethod]
		public void MergeDocText_TableWithConditionals_EmptyRowPreserved()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"

				| Item                                | In Stock                                  |
				| ----------------------------------- | ----------------------------------------- |
				| {{ if Name }}{{ Name }}{{ end if }} | {{ if InStock }}{{ InStock }}{{ end if }} |

				");

			var data = new
			{
				Name = "",
				InStock = "",
			};

			writer.Document.Merge(data);

			writer.Document.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				      </Paragraph>
				      <Table preferredwidth='Auto'>
				        <Row rowformat_preferredwidth='Auto'>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>Item</Run>
				            </Paragraph>
				          </Cell>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>In Stock</Run>
				            </Paragraph>
				          </Cell>
				        </Row>
				        <Row rowformat_preferredwidth='Auto'>
				          <Cell>
				          </Cell>
				          <Cell>
				          </Cell>
				        </Row>
				      </Table>
				      <Paragraph>
				      </Paragraph>
				      <Paragraph>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");
		}

		[TestMethod]
		public void MergeDocText_TableWithConditionals_RowRemainsIfOtherContent()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"

				| Item                                      | In Stock |
				| -----------------------------------       | -------- |
				| {{ if Name }}{{ Name }}{{ end if }}&nbsp; | Yes      |

				");

			var data = new
			{
				Name = "",
				InStock = "",
			};

			writer.Document.Merge(data);

			AssertText(writer.Document, @"

				| Item   | In Stock |
				| ----   | -------- |
				| &nbsp; | Yes      |

				", asMarkdown: true);

			Assert.AreEqual(2, writer.Document.SelectNodes("//Row").Count, "The row should be repeated for each item.");
			Assert.AreEqual(4, writer.Document.SelectNodes("//Cell").Count, "The cells should be repeated in each row.");
		}

		[TestMethod]
		public void MergeDocText_ListInSingleCellTableSameParagraph_RowIsRepeatedForEachItem()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"

				| Item                                     |
				| ---------------------------------------- |
				| {{ each Items }}{{ Name }}{{ end each }} |

				");

			var data = new
			{
				Items = new[] {
						new {Name = "Milk" },
						new {Name = "Eggs" },
						new {Name = "Artisan Bread" }
					},
			};

			writer.Document.Merge(data);

			AssertText(writer.Document, @"
				Item
				Milk
				Eggs
				Artisan Bread


				");

			Assert.AreEqual(4, writer.Document.SelectNodes("//Row").Count, "The row should be repeated for each item.");
			Assert.AreEqual(4, writer.Document.SelectNodes("//Cell").Count, "The cell should be repeated in each row.");
		}

		[TestMethod]
		public void MergeDocText_ListInSingleCellTableSameParagraphLeadingContent_RowIsRepeatedForEachItem()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"

				| Item                                              |
				| ------------------------------------------------- |
				| HEADER: {{ each Items }}{{ Name }},{{ end each }} |

				");

			var data = new
			{
				Items = new[] {
						new {Name = "Milk" },
						new {Name = "Eggs" },
						new {Name = "Artisan Bread" }
					},
			};

			writer.Document.Merge(data);

			AssertText(writer.Document, @"
				Item
				HEADER: Milk,Eggs,Artisan Bread,


				");

			Assert.AreEqual(2, writer.Document.SelectNodes("//Row").Count, "The row should not be repeated.");
			Assert.AreEqual(2, writer.Document.SelectNodes("//Cell").Count, "The cell should not be repeated.");
		}

		[TestMethod]
		public void MergeDocText_ListInSingleCellTableSameParagraphTrailingContent_RowIsRepeatedForEachItem()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"

				| Item                                                  |
				| ----------------------------------------------------- |
				| {{ each Items }}{{ Name }},{{ end each }} ... FOOTER |

				");

			var data = new
			{
				Items = new[] {
						new {Name = "Milk" },
						new {Name = "Eggs" },
						new {Name = "Artisan Bread" }
					},
			};

			writer.Document.Merge(data);

			AssertText(writer.Document, @"
				Item
				Milk,Eggs,Artisan Bread, ... FOOTER


				");

			Assert.AreEqual(2, writer.Document.SelectNodes("//Row").Count, "The row should not be repeated.");
			Assert.AreEqual(2, writer.Document.SelectNodes("//Cell").Count, "The cell should not be repeated.");
		}

		[TestMethod]
		public void MergeDocText_ListInSingleCellTableDifferentParagraphs_RowIsRepeatedForEachItem()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"

				| Item                                         |
				| -------------------------------------------- |
				| {{ each Items }}\n{{ Name }}\n{{ end each }} |

				");

			var data = new
			{
				Items = new[] {
						new {Name = "Milk" },
						new {Name = "Eggs" },
						new {Name = "Artisan Bread" }
					},
			};

			writer.Document.Merge(data);

			AssertText(writer.Document, @"
				Item
				Milk
				Eggs
				Artisan Bread


				");

			Assert.AreEqual(4, writer.Document.SelectNodes("//Row").Count, "The row should be repeated for each item.");
			Assert.AreEqual(4, writer.Document.SelectNodes("//Cell").Count, "The cell should be repeated in each row.");
		}

		[TestMethod]
		public void MergeDocText_ListInSingleCellTableDifferentParagraphsLeadingContent_RowIsRepeatedForEachItem()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"

				| Item                                                  |
				| ----------------------------------------------------- |
				| HEADER:\n{{ each Items }}\n{{ Name }}\n{{ end each }} |

				");

			var data = new
			{
				Items = new[] {
						new {Name = "Milk" },
						new {Name = "Eggs" },
						new {Name = "Artisan Bread" }
					},
			};

			writer.Document.Merge(data);

			AssertText(writer.Document, @"
				Item
				HEADER:
				Milk
				Eggs
				Artisan Bread


				");

			Assert.AreEqual(2, writer.Document.SelectNodes("//Row").Count, "The row should not be repeated.");
			Assert.AreEqual(2, writer.Document.SelectNodes("//Cell").Count, "The cell should not be repeated.");
		}

		[TestMethod]
		public void MergeDocText_ListInSingleCellTableDifferentParagraphsTrailingContent_RowIsRepeatedForEachItem()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"

				| Item                                                     |
				| -------------------------------------------------------- |
				| {{ each Items }}\n{{ Name }}\n{{ end each }}\n- FOOTER - |

				");

			var data = new
			{
				Items = new[] {
						new {Name = "Milk" },
						new {Name = "Eggs" },
						new {Name = "Artisan Bread" }
					},
			};

			writer.Document.Merge(data);

			AssertText(writer.Document, @"
				Item
				Milk
				Eggs
				Artisan Bread
				- FOOTER -


				");

			Assert.AreEqual(2, writer.Document.SelectNodes("//Row").Count, "The row should not be repeated.");
			Assert.AreEqual(2, writer.Document.SelectNodes("//Cell").Count, "The cell should not be repeated.");
		}

		[TestMethod]
		public void MergeDocText_ListInMultiRowTable_RowsAreRepeatedForEachItem()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"

				| Items                      |
				| -------------------------- |
				| {{ each Items }}{{ Name }} |
				| {{ InStock }}              |
				| {{ Price }}{{ end each }}  |

				");

			var data = new
			{
				Items = new[] {
						new {Name = "Milk", InStock = true, Price = 3.50 },
						new {Name = "Eggs", InStock = true, Price = 2.25 },
						new {Name = "Artisan Bread", InStock = false, Price = 6.99 }
					},
			};

			writer.Document.Merge(data);

			AssertText(writer.Document, @"
				Items
				Milk
				True
				3.5
				Eggs
				True
				2.25
				Artisan Bread
				False
				6.99


				");

			Assert.AreEqual(10, writer.Document.SelectNodes("//Row").Count, "The rows should be repeated for each item.");
			Assert.AreEqual(10, writer.Document.SelectNodes("//Cell").Count, "There should be one cell per row.");
		}

		[TestMethod]
		public void MergeDocText_ListInMultiRowTable_EmptyRowsRemoved()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"

				| Items                      |
				| -------------------------- |
				| {{ each Items }}{{ Name }} |
				| {{ InStock }}              |
				| {{ Price }}{{ end each }}  |

				");

			var data = new
			{
				Items = new object[0],
			};

			writer.Document.Merge(data);

			writer.Document.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				      </Paragraph>
				      <Table preferredwidth='Auto'>
				        <Row rowformat_preferredwidth='Auto'>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>Items</Run>
				            </Paragraph>
				          </Cell>
				        </Row>
				      </Table>
				      <Paragraph>
				      </Paragraph>
				      <Paragraph>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");

			Assert.AreEqual(1, writer.Document.SelectNodes("//Row").Count, "The header row should be the only one remaining.");
			Assert.AreEqual(1, writer.Document.SelectNodes("//Cell").Count, "There should be one cell per row.");
		}

		[TestMethod]
		public void MergeDocText_ListInSingleRowTable_EmptyRowPreserved()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"

				| Items                                                                |
				| -------------------------------------------------------------------- |
				| {{ each Items }}{{ Name }}, {{ InStock }}, {{ Price }}{{ end each }} |

				");

			var data = new
			{
				Items = new object[0],
			};

			writer.Document.Merge(data, keepEmptyRows: true);

			writer.Document.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				      </Paragraph>
				      <Table preferredwidth='Auto'>
				        <Row rowformat_preferredwidth='Auto'>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>Items</Run>
				            </Paragraph>
				          </Cell>
				        </Row>
				        <Row rowformat_preferredwidth='Auto'>
				          <Cell>
				          </Cell>
				        </Row>
				      </Table>
				      <Paragraph>
				      </Paragraph>
				      <Paragraph>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");
		}

		[TestMethod]
		public void MergeDocText_ListInMultiRowTable_EmptyRowPreserved()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"

				| Items                                  |
				| -------------------------------------- |
				| {{ each Items }}                       |
				| {{ Name }}, {{ InStock }}, {{ Price }} |
				| {{ end each }}                         |

				");

			var data = new
			{
				Items = new object[0],
			};

			writer.Document.Merge(data, keepEmptyRows: true);

			writer.Document.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				      </Paragraph>
				      <Table preferredwidth='Auto'>
				        <Row rowformat_preferredwidth='Auto'>
				          <Cell>
				            <Paragraph paragraphformat_spaceafter='6' paragraphformat_stylename='Body Text' paragraphformat_styleidentifier='BodyText'>
				              <Run>Items</Run>
				            </Paragraph>
				          </Cell>
				        </Row>
				        <Row rowformat_preferredwidth='Auto'>
				          <Cell>
				          </Cell>
				        </Row>
				      </Table>
				      <Paragraph>
				      </Paragraph>
				      <Paragraph>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");
		}

		[TestMethod]
		public void MergeDocText_ListWithCheckboxes()
		{
			var writer = new DocumentTemplateWriter();

			writer.InsertMarkdown(@"

				| Item                                                           |
				| -------------------------------------------------------------- |
				| {{ each Items }}{{ checkbox InStock }} {{ Name }}{{ end each }} |

				");

			var data = new
			{
				Items = new[] {
						new {Name = "Milk", InStock = true },
						new {Name = "Eggs", InStock = true },
						new {Name = "Artisan Bread", InStock = false }
					},
			};

			var generatorFactory = new KeywordGeneratorFactory<IGenerator<Document, Node, Type, object, string>>();

			generatorFactory.Generators.Add("checkbox", new CheckBoxSymbolGenerator<Type, object, string>());

			writer.Document.Merge(data, generatorFactory: generatorFactory);

			AssertText(writer.Document, @"
				Item
				þ Milk
				þ Eggs
				¨ Artisan Bread


				");

			Assert.AreEqual(4, writer.Document.SelectNodes("//Row").Count, "The row should be repeated for each item.");
			Assert.AreEqual(4, writer.Document.SelectNodes("//Cell").Count, "The cell should be repeated in each row.");
		}

		[TestMethod]
		public void MergeDocText_HyperlinkUrl()
		{
			var doc = new Document();

			var builder = new DocumentBuilder(doc);

			builder.InsertRun("^ ");

			builder.InsertField("HYPERLINK \"%7b%7bUrl%7d%7d\"");

			builder.InsertRun(" $");

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>^ </Run>
				        <FieldStart></FieldStart>
				        <Run>HYPERLINK ""%7b%7bUrl%7d%7d""</Run>
				        <FieldSeparator></FieldSeparator>
				        <FieldEnd></FieldEnd>
				        <Run> $</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");

			var data = new
			{
				Url = "https://www.google.com",
			};

			doc.Merge(data);

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>^ </Run>
				        <FieldStart></FieldStart>
				        <Run>HYPERLINK ""</Run>
				        <Run>https://www.google.com</Run>
				        <Run>""</Run>
				        <FieldSeparator></FieldSeparator>
				        <FieldEnd></FieldEnd>
				        <Run> $</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");
		}

		[TestMethod]
		public void MergeDocText_HyperlinkQueryString()
		{
			var doc = new Document();

			var builder = new DocumentBuilder(doc);

			builder.InsertRun("^ ");

			builder.InsertField("HYPERLINK \"https://google.com/q=%7b%7bQuery%7d%7d\"");

			builder.InsertRun(" $");

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>^ </Run>
				        <FieldStart></FieldStart>
				        <Run>HYPERLINK ""https://google.com/q=%7b%7bQuery%7d%7d""</Run>
				        <FieldSeparator></FieldSeparator>
				        <FieldEnd></FieldEnd>
				        <Run> $</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");

			var data = new
			{
				Query = "Arnold Schwarzenegger",
			};

			doc.Merge(data);

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>^ </Run>
				        <FieldStart></FieldStart>
				        <Run>HYPERLINK ""</Run>
				        <Run>https://google.com/q=</Run>
				        <Run>Arnold%20Schwarzenegger</Run>
				        <Run>""</Run>
				        <FieldSeparator></FieldSeparator>
				        <FieldEnd></FieldEnd>
				        <Run> $</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>
				");
		}

		[TestMethod]
		public void MergeDocText_RepeatableInHeaderAndFooter()
		{
			var doc = new Document();

			var builder = new DocumentBuilder(doc);

			builder.Write("{{ Name }}");

			builder.MoveToHeaderFooter(HeaderFooterType.HeaderPrimary);

			builder.Write("{{ each Items }}");

			builder.MoveToHeaderFooter(HeaderFooterType.FooterPrimary);

			builder.Write("{{ end each }}");

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>{{ Name }}</Run>
				      </Paragraph>
				    </Body>
				    <HeaderFooter headerfootertype='HeaderPrimary' islinkedtoprevious='False' storytype='PrimaryHeader'>
				      <Paragraph>
				        <Run>{{ each Items }}</Run>
				      </Paragraph>
				    </HeaderFooter>
				    <HeaderFooter headerfootertype='FooterPrimary' islinkedtoprevious='False' storytype='PrimaryFooter'>
				      <Paragraph>
				        <Run>{{ end each }}</Run>
				      </Paragraph>
				    </HeaderFooter>
				  </Section>
				</Document>");

			var data = new
			{
				Name = "Shopping List",
				Items = new[] {
						new {Name = "Milk" },
						new {Name = "Eggs" },
						new {Name = "Artisan Bread" }
					},
			};

			doc.Merge(data);

			doc.AssertXml(@"
				<Document>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>Milk</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>Eggs</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				  <Section>
				    <Body>
				      <Paragraph>
				        <Run>Artisan Bread</Run>
				      </Paragraph>
				    </Body>
				  </Section>
				</Document>");
		}

	}
}
