using System.Linq;
using ExoMerge.Analysis;
using ExoMerge.ModelExpressions.UnitTests.Models.Movies;
using ExoMerge.UnitTests.Assertions;
using ExoModel;
using ExoModel.Json;
using ExoModel.UnitTests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExoMerge.ModelExpressions.UnitTests
{
	[TestClass]
	[TestModel(Name = "Movies")]
	public class ModelExpressionParserTests
	{
		public TestContext TestContext { get; set; }

		protected JsonEntityContext Context { get; set; }

		private static readonly IExpressionParser<ModelType, ModelExpression> Parser = new ModelExpressionParser();

		[TestInitialize]
		public void Initialize()
		{
			Context = TestModel.Initialize(typeof(ModelExpressionParserTests), TestContext);
		}

		[TestCleanup]
		public void DisposeContext()
		{
			ModelContext.Current = null;
		}

		[TestMethod]
		public void TryParse_InvalidProperty()
		{
			var type = ModelContext.Current.GetModelType<Movie>();

			ModelExpression expr = null;

			AssertException.OfType<ModelExpression.ParseException>()
				.WithMessage("No property or field 'Ended' exists in type '" + typeof(Movie).FullName + "'")
				.IsThrownBy(() => expr = Parser.Parse(type, "Ended", null));

			Assert.IsNull(expr);
		}

		[TestMethod]
		public void TryParse_ValueProperty_Success()
		{
			var movie = Context.Fetch<Movie>(1);
			TestParse(movie, "Started", false, movie.Started, "5/14/2004");
		}

		[TestMethod]
		public void TryParse_ReferenceProperty_Success()
		{
			var movie = Context.Fetch<Movie>(1);
			TestParse(movie, "Director", true, movie.Director, "Ridley Scott");
		}

		[TestMethod]
		public void TryParse_InvalidLinqExpression()
		{
			var type = ModelContext.Current.GetModelType<Movie>();

			ModelExpression expr = null;

			AssertException.OfType<ModelExpression.ParseException>()
				.WithMessage("No applicable aggregate method 'All' exists")
				.IsThrownBy(() => expr = Parser.Parse(type, "Roles.All()", null));

			Assert.IsNull(expr);
		}

		[TestMethod]
		public void TryParse_ReferenceLinqExpression_Success()
		{
			var movie = Context.Fetch<Movie>(1);
			TestParse(movie, "Genres.First()", true, movie.Genres.First(), "Action");
		}

		[TestMethod]
		public void TryParse_ValueLinqExpression_Success()
		{
			var movie = Context.Fetch<Movie>(1);
			TestParse(movie, "Genres.Select(Name).First()", false, "Action", "Action");
		}

		private static void TestParse<TRoot, TResult>(TRoot root, string expression, bool isReference, TResult expectedResult, string expectedFormattedResult)
		{
			var rootType = ModelContext.Current.GetModelType<TRoot>();

			var expr = Parser.Parse(rootType, expression, null);

			Assert.IsNotNull(expr);

			var resultType = Parser.GetResultType(expr);

			if (isReference)
			{
				Assert.IsNotNull(resultType);
				Assert.AreEqual(typeof (TResult).FullName, resultType.Name);
			}
			else
			{
				Assert.IsNull(resultType);
			}

			var rootInstance = ((IModelInstance)root).Instance;

			Assert.AreEqual(expectedResult, expr.GetValue(rootInstance));

			Assert.AreEqual(expectedFormattedResult, expr.GetFormattedValue(rootInstance, null, null));
		}
	}
}
