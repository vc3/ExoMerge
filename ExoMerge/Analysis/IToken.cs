namespace ExoMerge.Analysis
{
	/// <summary>
	/// An object that represents a token and its corresponding textual value.
	/// </summary>
	public interface IToken
	{
		/// <summary>
		/// The textual value of the token.
		/// </summary>
		string Value { get; }
	}

	/// <summary>
	/// An object that represents a discrete token with a start and end, and its corresponding textual value.
	/// </summary>
	public interface IToken<out TStart, out TEnd> : IToken
	{
		/// <summary>
		/// The object that marks the start of the token.
		/// </summary>
		TStart Start { get; }

		/// <summary>
		/// The object that marks the end of the token.
		/// </summary>
		TEnd End { get; }
	}
}
