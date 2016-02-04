namespace ExoMerge.Analysis
{
	/// <summary>
	/// Represents a token's type in terms of a template
	/// structure/hierarchy made up of regions and fields.
	/// </summary>
	public enum TokenType
	{
		/// <summary>
		/// The token type has not be determined.
		/// </summary>
		Unknown,

		/// <summary>
		/// A content field.
		/// </summary>
		ContentField,

		/// <summary>
		/// The start of a repeatable region.
		/// </summary>
		RepeatableBegin,

		/// <summary>
		/// The end of a repeatable region.
		/// </summary>
		RepeatableEnd,

		/// <summary>
		/// The start of a conditional region (without a condition to test).
		/// </summary>
		ConditionalBegin,

		/// <summary>
		/// The start of a conditional region with a condition to test.
		/// </summary>
		ConditionalBeginWithTest,

		/// <summary>
		/// A conditonal region test option.
		/// </summary>
		ConditionalTest,

		/// <summary>
		/// A conditional region default option with no condition to test.
		/// </summary>
		ConditionalDefault,

		/// <summary>
		/// The end of a conditional region.
		/// </summary>
		ConditionalEnd,
	}
}