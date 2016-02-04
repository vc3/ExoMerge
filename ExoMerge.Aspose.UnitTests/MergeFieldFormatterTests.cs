using System;
using System.Collections.Generic;
using System.Linq;
using ExoMerge.Aspose.MergeFields;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.Aspose.UnitTests
{
	[TestClass]
	public class MergeFieldFormatterTests
	{
		[TestMethod]
		public void ExtractFormats_ExpressionOnly_NoFormats()
		{
			KeyValuePair<string, string>[] formats;
			var expression = MergeFieldFormatter.ExtractFormats("Description", out formats);
			Assert.AreEqual("Description", expression);
			Assert.AreEqual(0, formats.Length);
		}

		[TestMethod]
		public void ExtractFormats_ExpressionAndDateFormat_SingleFormat()
		{
			KeyValuePair<string, string>[] formats;
			var expression = MergeFieldFormatter.ExtractFormats("Origin.Date \\@ M/d/yyyy", out formats);
			Assert.AreEqual("Origin.Date", expression);
			Assert.AreEqual(1, formats.Length);
			Assert.AreEqual("@", formats.Single().Key);
			Assert.AreEqual("M/d/yyyy", formats.Single().Value);
		}

		[TestMethod]
		public void ApplyFormats_DateValue_DateFormat()
		{
			var formats = new[] {new KeyValuePair<string, string>("@", "M/d/yyyy")};
			var result = MergeFieldFormatter.ApplyFormats(DateTime.Today, formats);
			Assert.AreEqual(DateTime.Today.ToString("M/d/yyyy"), result);
		}

		[TestMethod]
		public void ApplyFormats_DateString_DateFormat()
		{
			var formats = new[] { new KeyValuePair<string, string>("@", "M/d/yyyy") };
			var result = MergeFieldFormatter.ApplyFormats("01/01/2010", formats);
			Assert.AreEqual("1/1/2010", result);
		}

		[TestMethod]
		public void ExtractFormats_ExpressionAndDateFormatQuoted_SingleFormat()
		{
			KeyValuePair<string, string>[] formats;
			var expression = MergeFieldFormatter.ExtractFormats("Origin.Date \\@ \"M/d/yyyy\"", out formats);
			Assert.AreEqual("Origin.Date", expression);
			Assert.AreEqual(1, formats.Length);
			Assert.AreEqual("@", formats.Single().Key);
			Assert.AreEqual("M/d/yyyy", formats.Single().Value);
		}

		[TestMethod]
		public void ExtractFormats_ExpressionAndNumericFormat_SingleFormat()
		{
			KeyValuePair<string, string>[] formats;
			var expression = MergeFieldFormatter.ExtractFormats("TotalCost \\# $#,##0.00", out formats);
			Assert.AreEqual("TotalCost", expression);
			Assert.AreEqual(1, formats.Length);
			Assert.AreEqual("#", formats.Single().Key);
			Assert.AreEqual("$#,##0.00", formats.Single().Value);
		}

		[TestMethod]
		public void ExtractFormats_ExpressionAndNumericFormatQuoted_SingleFormat()
		{
			KeyValuePair<string, string>[] formats;
			var expression = MergeFieldFormatter.ExtractFormats("TotalCost \\# \"$#,##0.00\"", out formats);
			Assert.AreEqual("TotalCost", expression);
			Assert.AreEqual(1, formats.Length);
			Assert.AreEqual("#", formats.Single().Key);
			Assert.AreEqual("$#,##0.00", formats.Single().Value);
		}

		[TestMethod]
		public void ApplyFormats_NumericValue_NumericFormat()
		{
			var formats = new[] { new KeyValuePair<string, string>("#", "$#,##0.00") };
			var result = MergeFieldFormatter.ApplyFormats(1032.4, formats);
			Assert.AreEqual("$1,032.40", result);
		}

		[TestMethod]
		public void ApplyFormats_NumericString_NumericFormat()
		{
			var formats = new[] { new KeyValuePair<string, string>("#", "$#,##0.00") };
			var result = MergeFieldFormatter.ApplyFormats("1032", formats);
			Assert.AreEqual("$1,032.00", result);
		}
	}
}
