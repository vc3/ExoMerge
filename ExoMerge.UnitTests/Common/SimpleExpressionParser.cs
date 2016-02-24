using System;
using System.Text.RegularExpressions;
using ExoMerge.Analysis;

namespace ExoMerge.UnitTests.Common
{
	/// <summary>
	/// Parses expression text as an expression object.
	/// </summary>
	public class SimpleExpressionParser : IExpressionParser<Type, string>
	{
		private readonly Regex parser;

		public SimpleExpressionParser()
		{
		}

		public SimpleExpressionParser(string parser)
		{
			this.parser = new Regex(parser);
		}

		public string Parse(Type sourceType, string text, Type resultType)
		{
			if (parser != null && !parser.IsMatch(text))
				throw new Exception("Invalid expression: " + text);

			return text;
		}

		public Type GetResultType(string expression)
		{
			return typeof(object);
		}
	}
}
