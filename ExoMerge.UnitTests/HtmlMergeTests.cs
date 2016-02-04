using ExoMerge.UnitTests.Extensions;
using HtmlAgilityPack;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.UnitTests
{
	[TestClass]
	public class HtmlMergeTests
	{
		[TestMethod]
		public void MergeHtml_RootField_NodesReplacedWithValue()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<p><span>{{ Business }}</span></p>
				");

			var data = new
			{
				Business = "Dave's Automotive",
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				<p><span>Dave's Automotive</span></p>
				");
		}

		[TestMethod]
		public void MergeHtml_EachRegion_MergeRepeatedPerItem()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<p>
					<span>{{ each States }}</span>
					<span>{{ Name }},</span>
					<span>{{ end each }}</span>
				</p>
				");

			var data = new
			{
				States = new[] {
						new {Name = "Wyoming" },
						new {Name = "Arkansas" },
						new {Name = "Vermont" }
					},
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				<p>
					<span>Wyoming</span>
					<span>,</span>
					<span>Arkansas</span>
					<span>,</span>
					<span>Vermont</span>
					<span>,</span>
				</p>
				");
		}

		//[TestMethod]
		public void MergeHtml_ListThatSpanParagraphs_MergeRepeatedPerItem()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<p>
					<span>{{ each SportsLeagues }}</span>
				</p>
				<p>
					<span>{{ Acronym }}</span>
				</p>
				<p>
					<span>{{ end each }}</span>
				</p>
				");

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

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				<p>
					<span>MLB</span>
				</p>
				<p>
					<span>NBA</span>
				</p>
				<p>
					<span>NFL</span>
				</p>
				<p>
					<span>NHL</span>
				</p>
				<p>
					<span>MLS/span>
				</p>
				");
		}

		//[TestMethod]
		public void MergeHtml_ListWithEmptyParagraphs_EmptyParagraphsArePreserved()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<p>^</p>
				<p>
					<span>{{ each CardinalDirections }}</span>
				</p>
				<p></p>
				<p>
					<span>{{ Symbol }}</span>
				</p>
				<p></p>
				<p>
					<span>{{ Name }}</span>
				</p>
				<p>
					<span>{{ end each }}</span>
				</p>
				<p></p>
				<p>$</p>
				");

			var data = new
			{
				CardinalDirections = new[] {
						new {Name = "North", Symbol='↑' },
						new {Name = "South", Symbol='↓' },
						new {Name = "East", Symbol='→' },
						new {Name = "West", Symbol='←' },
					},
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				<p>^</p>
				<p></p>
				<p>
					<span>↑</span>
				</p>
				<p></p>
				<p>
					<span>North</span>
				</p>
				<p></p>
				<p>
					<span>↓</span>
				</p>
				<p></p>
				<p>
					<span>South</span>
				</p>
				<p></p>
				<p>
					<span>→</span>
				</p>
				<p></p>
				<p>
					<span>East</span>
				</p>
				<p></p>
				<p>
					<span>←</span>
				</p>
				<p></p>
				<p>
					<span>West</span>
				</p>
				<p></p>
				<p>$</p>
				");
		}

		//[TestMethod]
		public void MergeHtml_ListThatSpanParagraphsNonStandard_MergeRepeatedPerItem()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<p>
					<span>^{{ each TimeZones }}</span>
				</p>
				<p>
					<span>{{ Name }}{{ end each }}$<span>
				</p>
				");

//				<p>
//					<span><span>
//				</p>

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

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"^
							Eastern
							Central
							Mountain West
							Pacific$
							");
		}

		[TestMethod]
		public void MergeHtml_IfHasValue_BlockIsRendered()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<p>
					<span>{{ if Recipients }}To: {{ each Recipients }}{{ Name }}, {{ end each }} Subject: {{ Title }}{{ end if }}</span>
				</p>
				");

			var data = new
			{
				Title = "Information About Your Policy",
				Recipients = new[] {
						new {Name = "Jane Doe" },
						new {Name = "Jon Doe" },
					},
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				<p>
					<span>To: </span><span>Jane Doe</span><span>, </span><span>Jon Doe</span><span>, </span><span> Subject: </span><span>Information About Your Policy</span>
				</p>
				");
		}

		[TestMethod]
		public void MergeHtml_IfFalseValue_BlockIsNotRendered()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<p>
					<span>{{ if ShouldSendOffer }}To: {{ Recipient }}{{ end if }}</span>
				</p>
				");

			var data = new
			{
				Title = "Free Vacation Offer",
				ShouldSendOffer = false,
				Recipient = "Valued Customer",
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				<p>
				</p>
				");
		}

		[TestMethod]
		public void MergeHtml_IfFalseValue_MultipleBlocksAreNotRendered()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<p>
					<span>^</span>
				</p>
				<p>
					<span>{{ if ShouldSendOffer }}</span>
				</p>
				<p>
					<span>To: {{ Recipient }}</span>
				</p>
				<p>
					<span>{{ end if }}</span>
				</p>
				<p>
					<span>$</span>
				</p>
				");

			var data = new
			{
				Title = "Free Vacation Offer",
				ShouldSendOffer = false,
				Recipient = "Valued Customer",
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				<p>
					<span>^</span>
				</p>
				<p>
				</p>
				<p>
				</p>
				<p>
					<span>$</span>
				</p>
				");
		}

		[TestMethod]
		public void MergeHtml_IfWithMultipleOptions_SatsifyingBlockIsRendered()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<p>
					<span>^</span>
				</p>
				<p>
					<span>{{ Title }}</span>
				</p>
				<p>
					<span>{{ if ShouldSendOffer }}</span>
				</p>
				<p>
					<span>To: {{ Recipient }}</span>
				</p>
				<p>
					<span>{{ else if IsUnderReview }}</span>
				</p>
				<p>
					<span>Status: &lt;UNDER_REVIEW&gt;</span>
				</p>
				<p>
					<span>{{ else }}</span>
				</p>
				<p>
					<span>Status: &lt;UNKNOWN&gt;</span>
				</p>
				<p>
					<span>{{ end if }}</span>
				</p>
				<p>
					<span>$</span>
				</p>
				");

			var data = new
			{
				Title = "Free Vacation Offer",
				IsUnderReview = true,
				ShouldSendOffer = false,
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				<p>
					<span>^</span>
				</p>
				<p>
					<span>Free Vacation Offer</span>
				</p>
				<p>
				</p>
				<p>
				</p>
				<p>
					<span>Status: &lt;UNDER_REVIEW&gt;</span>
				</p>
				<p>
				</p>
				<p>
				</p>
				<p>
					<span>$</span>
				</p>
				");
		}

		[TestMethod]
		public void MergeHtml_IfWithMultipleOptions_NonSatisfyingDefaultBlockIsRendered()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<p>
					<span>^</span>
				</p>
				<p>
					<span>{{ Title }}</span>
				</p>
				<p>
					<span>{{ if ShouldSendOffer }}</span>
				</p>
				<p>
					<span>To: {{ Recipient }}</span>
				</p>
				<p>
					<span>{{ else if IsUnderReview }}</span>
				</p>
				<p>
					<span>Status: &lt;UNDER_REVIEW&gt;</span>
				</p>
				<p>
					<span>{{ else }}</span>
				</p>
				<p>
					<span>Status: &lt;UNKNOWN&gt;</span>
				</p>
				<p>
					<span>{{ end if }}</span>
				</p>
				<p>
					<span>$</span>
				</p>
				");

			var data = new
			{
				Title = "Free Vacation Offer",
				IsUnderReview = false,
				ShouldSendOffer = false,
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				<p>
					<span>^</span>
				</p>
				<p>
					<span>Free Vacation Offer</span>
				</p>
				<p>
				</p>
				<p>
				</p>
				<p>
					<span>Status: &lt;UNKNOWN&gt;</span>
				</p>
				<p>
				</p>
				<p>
					<span>$</span>
				</p>
				");
		}

		[TestMethod]
		public void MergeHtml_IfWithinList_BlockIsRenderedWhenConditionPasses()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<p>
					<span>{{ each NcaaConferences }}{{ if Active }}{{ Name }}, {{ end if }}{{ end each }}</span>
				</p>
				");

			var data = new
			{
				NcaaConferences = new[] {
						new {Name = "PAC 12", Active = true },
						new {Name = "SEC", Active = true },
						new {Name = "Big East", Active = false }
					},
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				<p>
					<span>PAC 12</span>
					<span>, </span>
					<span>SEC</span>
					<span>, </span>
				</p>
				");
		}

		//[TestMethod]
		public void MergeHtml_ListInTable_RowIsRepeatedForEachItem()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
						<th>
							<span>In Stock</span>
						</th>
						<th>
							<span>Price</span>
						</th>
					</tr>
					<tr>
						<td>
							<span>{{ each Items }}{{ Name }}</span>
						</td>
						<td>
							<span>{{ InStock }}</span>
						</td>
						<td>
							<span>{{ Price }}{{ end each }}</span>
						</td>
					</tr>
				</table>
				");

			var data = new
			{
				Items = new[] {
						new {Name = "Milk", InStock = true, Price = 3.50 },
						new {Name = "Eggs", InStock = true, Price = 2.25 },
						new {Name = "Artisan Bread", InStock = false, Price = 6.99 }
					},
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
						<th>
							<span>In Stock</span>
						</th>
						<th>
							<span>Price</span>
						</th>
					</tr>
					<tr>
						<td>
							<span>Milk</span>
						</td>
						<td>
							<span>True</span>
						</td>
						<td>
							<span>3.50</span>
						</td>
					</tr>
					<tr>
						<td>
							<span>Eggs</span>
						</td>
						<td>
							<span>True</span>
						</td>
						<td>
							<span>2.25</span>
						</td>
					</tr>
					<tr>
						<td>
							<span>Artisan Bread</span>
						</td>
						<td>
							<span>False</span>
						</td>
						<td>
							<span>6.99</span>
						</td>
					</tr>
				</table>
				");
		}

		//[TestMethod]
		public void MergeHtml_ListInTable_RowIsRemovedIfNoItems()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
						<th>
							<span>In Stock</span>
						</th>
						<th>
							<span>Price</span>
						</th>
					</tr>
					<tr>
						<td>
							<span>{{ each Items }}{{ Name }}</span>
						</td>
						<td>
							<span>{{ InStock }}</span>
						</td>
						<td>
							<span>{{ Price }}{{ end each }}</span>
						</td>
					</tr>
				</table>
				");

			var data = new
			{
				Items = new object[0],
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
						<th>
							<span>In Stock</span>
						</th>
						<th>
							<span>Price</span>
						</th>
					</tr>
				</table>
				");
		}

		[TestMethod]
		public void MergeHtml_ListInTableWithBlankValues_RowRemains()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
						<th>
							<span>In Stock</span>
						</th>
						<th>
							<span>Price</span>
						</th>
					</tr>
					<tr>
						<td>
							<span>{{ each Items }}{{ Name }}</span>
						</td>
						<td>
							<span>{{ InStock }}</span>
						</td>
						<td>
							<span>{{ Price }}{{ end each }}</span>
						</td>
					</tr>
				</table>
				");

			var data = new
			{
				Items = new[]
				{
					new {Name = "", InStock = "", Price = ""},
				},
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
						<th>
							<span>In Stock</span>
						</th>
						<th>
							<span>Price</span>
						</th>
					</tr>
					<tr>
						<td>
							<span></span>
						</td>
						<td>
							<span></span>
						</td>
						<td>
							<span></span>
						</td>
					</tr>
				</table>
				");
		}

		[TestMethod]
		public void MergeHtml_ListInTableWithConditionals_RowIsRemovedIfEmpty()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
						<th>
							<span>In Stock</span>
						</th>
					</tr>
					<tr>
						<td>
							<span>{{ if Name }}{{ Name }}{{ end if }}</span>
						</td>
						<td>
							<span>{{ if InStock }}{{ InStock }}{{ end if }}</span>
						</td>
					</tr>
				</table>
				");

			var data = new
			{
				Name = "",
				InStock = "",
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
						<th>
							<span>In Stock</span>
						</th>
					</tr>
					<tr>
						<td>
						</td>
						<td>
						</td>
					</tr>
				</table>
				");
		}

		[TestMethod]
		public void MergeHtml_ListInTableWithConditionals_RowRemainsIfOtherContent()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
						<th>
							<span>In Stock</span>
						</th>
					</tr>
					<tr>
						<td>
							<span>{{ if Name }}{{ Name }}{{ end if }}</span>
						</td>
						<td>
							<span>Yes</span>
						</td>
					</tr>
				</table>
				");

			var data = new
			{
				Name = "",
				InStock = "",
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
						<th>
							<span>In Stock</span>
						</th>
					</tr>
					<tr>
						<td>
						</td>
						<td>
							<span>Yes</span>
						</td>
					</tr>
				</table>
				");
		}

		[TestMethod]
		public void MergeHtml_ListInSingleCellTableSameParagraph_RowIsRepeatedForEachItem()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
					</tr>
					<tr>
						<td>
							<span>{{ each Items }}{{ Name }}{{ end each }}</span>
						</td>
					</tr>
				</table>
				");

			var data = new
			{
				Items = new[] {
						new {Name = "Milk" },
						new {Name = "Eggs" },
						new {Name = "Artisan Bread" }
					},
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
					</tr>
					<tr>
						<td>
							<span>Milk</span>
							<span>Eggs</span>
							<span>Artisan Bread</span>
						</td>
					</tr>
				</table>
				");
		}

		[TestMethod]
		public void MergeHtml_ListInSingleCellTableSameParagraphLeadingContent_RowIsRepeatedForEachItem()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
					</tr>
					<tr>
						<td>
							<span>HEADER: {{ each Items }}{{ Name }},{{ end each }}</span>
						</td>
					</tr>
				</table>
				");

			var data = new
			{
				Items = new[] {
						new {Name = "Milk" },
						new {Name = "Eggs" },
						new {Name = "Artisan Bread" }
					},
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
					</tr>
					<tr>
						<td>
							<span>HEADER: </span>
							<span>Milk</span>
							<span>,</span>
							<span>Eggs</span>
							<span>,</span>
							<span>Artisan Bread</span>
							<span>,</span>
						</td>
					</tr>
				</table>
				");
		}

		[TestMethod]
		public void MergeHtml_ListInSingleCellTableSameParagraphTrailingContent_RowIsRepeatedForEachItem()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
					</tr>
					<tr>
						<td>
							<span>{{ each Items }}{{ Name }},{{ end each }} ... FOOTER</span>
						</td>
					</tr>
				</table>
				");

			var data = new
			{
				Items = new[] {
						new {Name = "Milk" },
						new {Name = "Eggs" },
						new {Name = "Artisan Bread" }
					},
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
					</tr>
					<tr>
						<td>
							<span>Milk</span>
							<span>,</span>
							<span>Eggs</span>
							<span>,</span>
							<span>Artisan Bread</span>
							<span>,</span>
							<span> ... FOOTER</span>
						</td>
					</tr>
				</table>
				");
		}

		//[TestMethod]
		public void MergeHtml_ListInSingleCellTableDifferentParagraphs_RowIsRepeatedForEachItem()
		{
			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
					</tr>
					<tr>
						<td>
							<p>
								<span>{{ each Items }}</span>
							</p>
							<p>
								<span>{{ Name }}</span>
							</p>
							<p>
								<span>{{ end each }}</span>
							</p>
						</td>
					</tr>
				</table>
				");

			var data = new
			{
				Items = new[] {
						new {Name = "Milk" },
						new {Name = "Eggs" },
						new {Name = "Artisan Bread" }
					},
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
					</tr>
					<tr>
						<td>
							<p>
								<span>Milk</span><span>,</span>
							</p>
						</td>
					</tr>
					<tr>
						<td>
							<p>
								<span>Eggs</span><span>,</span>
							</p>
						</td>
					</tr>
					<tr>
						<td>
							<p>
								<span>Artisan Bread</span><span>,</span>
							</p>
						</td>
					</tr>
				</table>
				");
		}

		/*
		[TestMethod]
		public void MergeHtml_ListInSingleCellTableDifferentParagraphsLeadingContent_RowIsRepeatedForEachItem()
		{
			writer.InsertMarkdown(@"

				| Item                                                  |
				| ----------------------------------------------------- |
				| HEADER:\n{{ each Items }}\n{{ Name }}\n{{ end each }} |

				");

			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
					</tr>
					<tr>
						<td>
							<p>
								<span>HEADER:</span>
							</p>
							<p>
								<span>{{ each Items }}</span>
							</p>
							<p>
								<span>{{ Name }}</span>
							</p>
							<p>
								<span>{{ end each }}</span>
							</p>
						</td>
					</tr>
				</table>
				");

			var data = new
			{
				Items = new[] {
						new {Name = "Milk" },
						new {Name = "Eggs" },
						new {Name = "Artisan Bread" }
					},
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				Item
				HEADER:
				Milk
				Eggs
				Artisan Bread


				");
		}

		[TestMethod]
		public void MergeHtml_ListInSingleCellTableDifferentParagraphsTrailingContent_RowIsRepeatedForEachItem()
		{
			writer.InsertMarkdown(@"

				| Item                                                     |
				| -------------------------------------------------------- |
				| {{ each Items }}\n{{ Name }}\n{{ end each }}\n- FOOTER - |

				");

			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<table>
					<tr>
						<th>
							<span>Item</span>
						</th>
					</tr>
					<tr>
						<td>
							<p>
								<span>{{ each Items }}</span>
							</p>
							<p>
								<span>{{ Name }}</span>
							</p>
							<p>
								<span>{{ end each }}</span>
							</p>
						</td>
					</tr>
				</table>
				");

			var data = new
			{
				Items = new[] {
						new {Name = "Milk" },
						new {Name = "Eggs" },
						new {Name = "Artisan Bread" }
					},
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				Item
				Milk
				Eggs
				Artisan Bread
				- FOOTER -


				");
		}

		[TestMethod]
		public void MergeHtml_ListInMultiRowTable_RowsAreRepeatedForEachItem()
		{
			writer.InsertMarkdown(@"

				| Items                      |
				| -------------------------- |
				| {{ each Items }}{{ Name }} |
				| {{ InStock }}              |
				| {{ Price }}{{ end each }}  |

				");

			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<p>
					<span>{{ Business }}</span>
				</p>
				);

			var data = new
			{
				Items = new[] {
						new {Name = "Milk", InStock = true, Price = 3.50 },
						new {Name = "Eggs", InStock = true, Price = 2.25 },
						new {Name = "Artisan Bread", InStock = false, Price = 6.99 }
					},
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
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
		}

		[TestMethod]
		public void MergeHtml_ListInMultiRowTable_RowsAreRemovedIfNoItems()
		{
			writer.InsertMarkdown(@"

				| Items                      |
				| -------------------------- |
				| {{ each Items }}{{ Name }} |
				| {{ InStock }}              |
				| {{ Price }}{{ end each }}  |

				");

			var doc = new HtmlDocument();

			doc.LoadHtml(@"
				<p>
					<span>{{ Business }}</span>
				</p>
				);

			var data = new
			{
				Items = new object[0],
			};

			IMergeError[] errors;

			Assert.IsTrue(doc.TryMerge(data, out errors));

			doc.AssertMatch(@"
				Items


				");
		}
		*/
	}
}
