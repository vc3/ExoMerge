using System;
using System.Linq;
using ExoMerge.Analysis;
using ExoMerge.Aspose.MergeFields;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.Aspose.UnitTests
{
	[TestClass]
	public class MergeFieldPrefixParserTests
	{
		private static readonly ITokenParser<Type> Parser = new MergeFieldPrefixParser<Type>();

		[TestMethod]
		public void TryParse_SingleProperty_ExpressionOnly()
		{
			var result = Parser.Parse(typeof(object), "Value");
			Assert.AreEqual(TokenType.ContentField, result.Type);
			Assert.AreEqual("Value", result.Value);
		}

		[TestMethod]
		public void TryParse_PropertyPath_ExpressionOnly()
		{
			var result = Parser.Parse(typeof (object), "Item.Value");
			Assert.AreEqual(TokenType.ContentField, result.Type);
			Assert.AreEqual("Item.Value", result.Value);
		}

		[TestMethod]
		public void TryParse_PrefixAndProperty_CommandAndExpression()
		{
			var result = Parser.Parse(typeof (object), "TableStart:Group.Items");
			Assert.AreEqual(TokenType.RepeatableBegin, result.Type);
			Assert.AreEqual("Group.Items", result.Value);
		}

		[TestMethod]
		public void TryParse_PrefixPropertyAndSwitch_CommandExpressionAndSwitches()
		{
			var result = Parser.Parse(typeof(object), " TableStart:Group.Items \\where Enabled = True ");
			Assert.AreEqual(TokenType.RepeatableBegin, result.Type);
			Assert.AreEqual("Group.Items", result.Value);
			Assert.AreEqual("where:Enabled = True", string.Join(",", result.Options.OrderBy(o => o.Key).Select(o => string.Format(o.Key + ":" + o.Value))));
		}

		[TestMethod]
		public void TryParse_ExpressionWithSpecialCharacter_Allowed()
		{
			var result = Parser.Parse(typeof(object), "It\\em.Value");
			Assert.AreEqual(TokenType.ContentField, result.Type);
			Assert.AreEqual("It\\em.Value", result.Value);
		}

		[TestMethod]
		public void TryParse_PrefixWithTrailingSpaces_SpacesIgnored()
		{
			var result = Parser.Parse(typeof(object), "TableStart:  Group.Items");
			Assert.AreEqual(TokenType.RepeatableBegin, result.Type);
			Assert.AreEqual("Group.Items", result.Value);
		}
	}
}
