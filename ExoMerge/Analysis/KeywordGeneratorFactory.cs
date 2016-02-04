using System.Collections.Generic;

namespace ExoMerge.Analysis
{
	/// <summary>
	/// Returns a generator if the token value begins with a
	/// keyword that has been assigned to the generator.
	/// </summary>
	public class KeywordGeneratorFactory<TGenerator> : IGeneratorFactory<TGenerator>
		where TGenerator : class
	{
		private readonly KeywordPrefixParser parser = new KeywordPrefixParser();

		/// <summary>
		/// Creates a new keyword generator factory.
		/// </summary>
		public KeywordGeneratorFactory()
		{
			Generators = new Dictionary<string, TGenerator>();
		}

		/// <summary>
		/// Gets a dictionary of keywords and generators.
		/// </summary>
		public IDictionary<string, TGenerator> Generators { get; private set; }

		/// <summary>
		/// Gets a generator for the given token value. Also returns as an out
		/// parameter the portion of the token value that the generator will
		/// use to generate content.
		/// </summary>
		/// <param name="value">The token value.</param>
		/// <param name="expression">The portion of the token value that the generator will use to generate content.</param>
		/// <returns>A generator to use for the given token.</returns>
		public TGenerator GetGenerator(string value, out string expression)
		{
			string keyword;
			string remainder;

			if (!parser.TryParse(value, out keyword, out remainder))
			{
				expression = null;
				return null;
			}

			TGenerator generator;
			if (!Generators.TryGetValue(keyword, out generator))
			{
				expression = null;
				return null;
			}

			expression = remainder.Trim();
			return generator;
		}
	}
}
