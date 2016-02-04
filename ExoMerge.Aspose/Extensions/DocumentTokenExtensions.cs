using Aspose.Words;
using ExoMerge.Documents;

namespace ExoMerge.Aspose.Extensions
{
	public static class DocumentTokenExtensions
	{
		/// <summary>
		/// Determines whether the given field is the old thing left in its parent.
		/// </summary>
		public static bool IsAloneInParent(this DocumentToken<Node> token)
		{
			var start = token.Start;
			var end = token.End;

			if (start.ParentNode != end.ParentNode)
				return false;

			var parent = start.ParentNode;

			return parent.FirstChild == start && parent.LastChild == end;
		}
	}
}
