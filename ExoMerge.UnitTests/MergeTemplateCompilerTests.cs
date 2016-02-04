using System;
using System.Collections.Generic;
using System.Linq;
using ExoMerge.Analysis;
using ExoMerge.Structure;
using ExoMerge.UnitTests.Common;
using ExoMerge.UnitTests.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.UnitTests
{
	[TestClass]
	public class MergeTemplateCompilerTests
	{
		private static readonly ITokenParser<Type> TokenParser = new SimpleTokenParser();

		private static readonly IExpressionParser<Type, string> ExpressionParser = new SimpleExpressionParser();

		private static IEnumerable<SimpleToken> CreateTokens(IEnumerable<string> tokens)
		{
			return tokens.Select(SimpleToken.Create).ToArray();
		}

		[TestMethod]
		public void Compile_SingleFieldToken_RegionWithOneField()
		{
			var root = MergeTemplateCompiler.Compile<string, string, SimpleToken, Type, object, string>(typeof(object), TokenParser, ExpressionParser, CreateTokens(new[]
			{
				"SimpleField"
			}));

			Assert.AreEqual(1, root.Children.OfType<Field<string, string, SimpleToken, Type, object, string>>().Count());
			Assert.AreEqual(0, root.Children.OfType<IRegion<SimpleToken>>().Count());

			Assert.AreEqual("SimpleField", ((Field<string, string, SimpleToken, Type, object, string>)root.Children.Single()).Token.Value);
		}

		[TestMethod]
		public void Compile_EachWithSingleFieldToken_RepeatableWithOneField()
		{
			var root = MergeTemplateCompiler.Compile<string, string, SimpleToken, Type, object, string>(typeof(object), TokenParser, ExpressionParser, CreateTokens(new[]
			{
				"each Item",
				"SimpleField",
				"end each"
			}));

			Assert.AreEqual(0, root.Children.OfType<Field<string, string, SimpleToken, Type, object, string>>().Count());
			Assert.AreEqual(1, root.Children.OfType<IRegion<SimpleToken>>().Count());

			Assert.IsInstanceOfType(root.Children.OfType<IRegion<SimpleToken>>().Single(), typeof(Repeatable<string, string, SimpleToken, Type, object, string>));
			Assert.AreEqual(1, ((Repeatable<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Children.OfType<Field<string, string, SimpleToken, Type, object, string>>().Count());
			Assert.AreEqual(0, ((Repeatable<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Children.OfType<IRegion<SimpleToken>>().Count());

			Assert.AreEqual("each Item", root.Children.OfType<IRegion<SimpleToken>>().Single().StartToken.Value);

			Assert.AreEqual("SimpleField", ((Repeatable<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Children.OfType<Field<string, string, SimpleToken, Type, object, string>>().Single().Token.Value);

			Assert.AreEqual("end each", root.Children.OfType<IRegion<SimpleToken>>().Single().EndToken.Value);
		}

		[TestMethod]
		public void Compile_IfWithSingleFieldToken_ConditioanlOptionWithOneField()
		{
			var root = MergeTemplateCompiler.Compile<string, string, SimpleToken, Type, object, string>(typeof(object), TokenParser, ExpressionParser, CreateTokens(new[]
			{
				"if IsActive",
				"SimpleField",
				"end if"
			}));

			Assert.AreEqual(0, root.Children.OfType<Field<string, string, SimpleToken, Type, object, string>>().Count());
			Assert.AreEqual(1, root.Children.OfType<IRegion<SimpleToken>>().Count());

			Assert.IsInstanceOfType(root.Children.OfType<IRegion<SimpleToken>>().Single(), typeof(Conditional<string, string, SimpleToken, Type, object, string>));
			Assert.AreEqual(1, ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.Count());
			Assert.IsInstanceOfType(((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(0), typeof(Conditional<string, string, SimpleToken, Type, object, string>.TestOption));

			Assert.AreEqual(1, ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(0).Children.OfType<Field<string, string, SimpleToken, Type, object, string>>().Count());
			Assert.AreEqual(0, ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(0).Children.OfType<IRegion<SimpleToken>>().Count());

			Assert.AreEqual("if IsActive", root.Children.OfType<IRegion<SimpleToken>>().Single().StartToken.Value);

			Assert.AreEqual("if IsActive", ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.Single().StartToken.Value);
			Assert.AreEqual("SimpleField", ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(0).Children.OfType<Field<string, string, SimpleToken, Type, object, string>>().Single().Token.Value);

			Assert.AreEqual("end if", root.Children.OfType<IRegion<SimpleToken>>().Single().EndToken.Value);
		}

		[TestMethod]
		public void Compile_IfAndElseWithSingleFieldToken_ConditionalOptionWithOneFieldAndDefaultWithOneField()
		{
			var root = MergeTemplateCompiler.Compile<string, string, SimpleToken, Type, object, string>(typeof(object), TokenParser, ExpressionParser, CreateTokens(new[]
			{
				"if IsActive",
				"SimpleField",
				"else",
				"DeactivatedDate",
				"end if"
			}));

			Assert.AreEqual(0, root.Children.OfType<Field<string, string, SimpleToken, Type, object, string>>().Count());
			Assert.AreEqual(1, root.Children.OfType<IRegion<SimpleToken>>().Count());

			Assert.IsInstanceOfType(root.Children.OfType<IRegion<SimpleToken>>().Single(), typeof(Conditional<string, string, SimpleToken, Type, object, string>));
			Assert.AreEqual(2, ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.Count());
			Assert.IsInstanceOfType(((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(0), typeof(Conditional<string, string, SimpleToken, Type, object, string>.TestOption));
			Assert.IsInstanceOfType(((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(1), typeof(Conditional<string, string, SimpleToken, Type, object, string>.DefaultOption));

			Assert.AreEqual(1, ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(0).Children.OfType<Field<string, string, SimpleToken, Type, object, string>>().Count());
			Assert.AreEqual(0, ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(0).Children.OfType<IRegion<SimpleToken>>().Count());
			Assert.AreEqual(1, ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(1).Children.OfType<Field<string, string, SimpleToken, Type, object, string>>().Count());
			Assert.AreEqual(0, ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(1).Children.OfType<IRegion<SimpleToken>>().Count());

			Assert.AreEqual("if IsActive", root.Children.OfType<IRegion<SimpleToken>>().Single().StartToken.Value);

			Assert.AreEqual("if IsActive", ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(0).StartToken.Value);
			Assert.AreEqual("SimpleField", ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(0).Children.OfType<Field<string, string, SimpleToken, Type, object, string>>().Single().Token.Value);

			Assert.AreEqual("else", ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(1).StartToken.Value);
			Assert.AreEqual("DeactivatedDate", ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(1).Children.OfType<Field<string, string, SimpleToken, Type, object, string>>().Single().Token.Value);

			Assert.AreEqual("end if", root.Children.OfType<IRegion<SimpleToken>>().Single().EndToken.Value);
		}

		[TestMethod]
		public void Compile_IfElseIfAndElseWithSingleFieldToken_RegionWithOneField()
		{
			var root = MergeTemplateCompiler.Compile<string, string, SimpleToken, Type, object, string>(typeof(object), TokenParser, ExpressionParser, CreateTokens(new[]
			{
				"if IsActive",
				"SimpleField",
				"else if IsDeleted",
				"DeletedDate",
				"else",
				"DeactivatedDate",
				"end if"
			}));

			Assert.AreEqual(0, root.Children.OfType<Field<string, string, SimpleToken, Type, object, string>>().Count());
			Assert.AreEqual(1, root.Children.OfType<IRegion<SimpleToken>>().Count());

			Assert.IsInstanceOfType(root.Children.OfType<IRegion<SimpleToken>>().Single(), typeof(Conditional<string, string, SimpleToken, Type, object, string>));
			Assert.AreEqual(3, ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.Count());
			Assert.IsInstanceOfType(((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(0), typeof(Conditional<string, string, SimpleToken, Type, object, string>.TestOption));
			Assert.IsInstanceOfType(((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(1), typeof(Conditional<string, string, SimpleToken, Type, object, string>.TestOption));
			Assert.IsInstanceOfType(((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(2), typeof(Conditional<string, string, SimpleToken, Type, object, string>.DefaultOption));

			Assert.AreEqual(1, ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(0).Children.OfType<Field<string, string, SimpleToken, Type, object, string>>().Count());
			Assert.AreEqual(0, ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(0).Children.OfType<IRegion<SimpleToken>>().Count());
			Assert.AreEqual(1, ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(1).Children.OfType<Field<string, string, SimpleToken, Type, object, string>>().Count());
			Assert.AreEqual(0, ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(1).Children.OfType<IRegion<SimpleToken>>().Count());
			Assert.AreEqual(1, ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(2).Children.OfType<Field<string, string, SimpleToken, Type, object, string>>().Count());
			Assert.AreEqual(0, ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(2).Children.OfType<IRegion<SimpleToken>>().Count());

			Assert.AreEqual("if IsActive", root.Children.OfType<IRegion<SimpleToken>>().Single().StartToken.Value);

			Assert.AreEqual("if IsActive", ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(0).StartToken.Value);
			Assert.AreEqual("SimpleField", ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(0).Children.OfType<Field<string, string, SimpleToken, Type, object, string>>().Single().Token.Value);

			Assert.AreEqual("else if IsDeleted", ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(1).StartToken.Value);
			Assert.AreEqual("DeletedDate", ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(1).Children.OfType<Field<string, string, SimpleToken, Type, object, string>>().Single().Token.Value);

			Assert.AreEqual("else", ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(2).StartToken.Value);
			Assert.AreEqual("DeactivatedDate", ((Conditional<string, string, SimpleToken, Type, object, string>)root.Children.OfType<IRegion<SimpleToken>>().Single()).Options.ElementAt(2).Children.OfType<Field<string, string, SimpleToken, Type, object, string>>().Single().Token.Value);

			Assert.AreEqual("end if", root.Children.OfType<IRegion<SimpleToken>>().Single().EndToken.Value);
		}
	}
}
