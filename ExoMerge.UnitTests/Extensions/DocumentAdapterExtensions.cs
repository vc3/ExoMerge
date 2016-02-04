using System.Collections.Generic;
using System.Text;
using ExoMerge.Documents;

namespace ExoMerge.UnitTests.Extensions
{
	public static class DocumentAdapterExtensions
	{
		public static string GetMarkdown<TDocument, TNode>(this IDocumentAdapter<TDocument, TNode> adapter, TDocument document)
		{
			var result = new StringBuilder();

			WriteMarkdown(result, adapter, adapter.GetChildren(document));

			return result.ToString();
		}

		private static void WriteMarkdown<TDocument, TNode>(StringBuilder builder, IDocumentAdapter<TDocument, TNode> adapter, IEnumerable<TNode> nodes)
		{
			foreach (var node in nodes)
			{
				if (adapter.IsComposite(node) && builder.Length > 0)
				{
					builder.AppendLine();
					builder.AppendLine();
				}

				if (adapter.GetNodeType(node) == DocumentNodeType.Run)
					builder.Append(adapter.GetText(node));

				if (adapter.IsComposite(node))
					WriteMarkdown(builder, adapter, adapter.GetChildren(node));
			}
		}
	}
}
