namespace ExoMerge.Analysis
{
	/// <summary>
	/// An object that assigns a generator based on a token value.
	/// </summary>
	/// <typeparam name="TGenerator">The type of generator to assign.</typeparam>
	public interface IGeneratorFactory<out TGenerator>
		where TGenerator : class
	{
		/// <summary>
		/// Gets a generator for the given token value. Also returns as an out
		/// parameter the portion of the token value that the generator will
		/// use to generate content.
		/// </summary>
		/// <remarks>
		/// An example implementation is the `KeywordGeneratorFactory`,
		/// which looks for a keyword prefix that matches a known generator and
		/// returns the remainder of the text as the generator's input expression.
		/// </remarks>
		/// <param name="value">The token value.</param>
		/// <param name="expression">The portion of the token value that the generator will use to generate content.</param>
		/// <returns>A generator to use for the given token.</returns>
		TGenerator GetGenerator(string value, out string expression);
	}
}
