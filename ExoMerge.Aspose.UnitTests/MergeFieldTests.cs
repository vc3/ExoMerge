using Aspose.Words;
using Aspose.Words.Fields;
using ExoMerge.Aspose.MergeFields;
using ExoMerge.Aspose.UnitTests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.Aspose.UnitTests
{
	[TestClass]
	public class MergeFieldTests
	{
		[TestMethod]
		public void TryParse_SimpleMergeField_Successful()
		{
			Field[] fields;
			DocumentConverter.FromDisplayCode("{ MERGEFIELD Name }", out fields);

			MergeField mergeField;
			Assert.IsTrue(MergeField.TryParse(fields[0].Start, out mergeField), "Could not parse merge field.");
			Assert.AreEqual(" MERGEFIELD Name ", mergeField.GetCode());
			Assert.AreEqual("Name", mergeField.Name);
		}

		[TestMethod]
		public void TryParse_NonMergeField_ReturnsFalse()
		{
			var builder = new DocumentBuilder();
			var field = builder.InsertField(" IF \"True\"=\"True\" \"Yes\" \"No\" ");
			var fieldStart = field.Start;

			MergeField mergeField;
			Assert.IsFalse(MergeField.TryParse(fieldStart, out mergeField), "Should not be able to parse an IF field as a merge field.");
		}
	}
}
