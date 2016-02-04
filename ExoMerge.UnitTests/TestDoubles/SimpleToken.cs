using ExoMerge.Analysis;

namespace ExoMerge.UnitTests.TestDoubles
{
	public class SimpleToken : IToken
	{
		private SimpleToken(string value)
		{
			Value = value;
		}

		public static SimpleToken Create(string value)
		{
			return new SimpleToken(value);
		}

		public string Value { get; private set; }
	}
}
