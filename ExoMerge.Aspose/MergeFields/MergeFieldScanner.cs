using System.Collections.Generic;
using Aspose.Words;
using Aspose.Words.Fields;
using ExoMerge.Analysis;
using ExoMerge.Documents;

namespace ExoMerge.Aspose.MergeFields
{
	/// <summary>
	/// Scans a document and identifies tokens as merge fields.
	/// </summary>
	public class MergeFieldScanner : ITemplateScanner<Document, DocumentToken<Node>>
	{
		#region Methods

		/// <summary>
		/// Scan nodes for merge fields and return them as tokens.
		/// </summary>
		private static IEnumerable<DocumentToken<Node>> ScanNodes(CompositeNode composite)
		{
			var node = composite.FirstChild;

			if (node == null)
				yield break;

			do
			{
				var fieldStart = node as FieldStart;

				if (fieldStart != null)
				{
					MergeField field;
					if (MergeField.TryParse(fieldStart, out field))
					{
						var end = field.GetEnd();

						if (end != null)
						{
							yield return new DocumentToken<Node>(field.Start, end, field.Name);

							node = end.NextSibling;
						}
						else
						{
							node = node.NextSibling;
						}
					}
					else
					{
						node = node.NextSibling;
					}
				}
				else
				{
					if (node.IsComposite)
						foreach (var token in ScanNodes((CompositeNode) node))
							yield return token;

					node = node.NextSibling;
				}
			} while (node != null);
		}

		#endregion

		#region IDocumentScanner<Document>

		IEnumerable<DocumentToken<Node>> ITemplateScanner<Document, DocumentToken<Node>>.GetTokens(Document template)
		{
			return ScanNodes(template);
		}

		#endregion
	}
}
