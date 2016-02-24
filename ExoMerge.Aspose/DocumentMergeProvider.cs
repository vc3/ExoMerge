using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Aspose.Words;
using Aspose.Words.Drawing;
using ExoMerge.Analysis;
using ExoMerge.Aspose.Common;
using ExoMerge.Aspose.Extensions;
using ExoMerge.DataAccess;
using ExoMerge.Documents;
using ExoMerge.Rendering;
using ExoMerge.Structure;

namespace ExoMerge.Aspose
{
	/// <summary>
	/// An object which can be used to produce a merged document from a given document template and data context.
	/// </summary>
	/// <typeparam name="TSourceType">The type that represents the type of source data, e.g. 'Type'.</typeparam>
	/// <typeparam name="TSource">The type of the source data to merge, e.g. 'Object'.</typeparam>
	/// <typeparam name="TExpression">The type of parsed expressions.</typeparam>
	public abstract class DocumentMergeProvider<TSourceType, TSource, TExpression> : DocumentMergeProvider<Document, Node, TSourceType, TSource, TExpression>
		where TSourceType : class
		where TExpression : class
	{
		private const string HtmlTextExpr = @"(</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>)|(&([a-z]+|#x[0-9a-f]+|#[0-9]+);)";

		private const string DataUriExpr = @"^data:image/(?<extension>[A-Za-z]+);base64,(?<content>.+)$";

		/// <summary>
		/// Creates a new merge provider.
		/// </summary>
		/// <param name="scanner">An object used to find tokens in the document.</param>
		/// <param name="expressionParser">The object to use to parse the raw expression text.</param>
		/// <param name="generatorFactory">An object that can assign custom generators.</param>
		/// <param name="dataProvider">An object used to access source data while merging.</param>
		protected DocumentMergeProvider(ITemplateScanner<Document, DocumentToken<Node>> scanner, IExpressionParser<TSourceType, TExpression> expressionParser, IGeneratorFactory<IGenerator<Document, Node, TSourceType, TSource, TExpression>> generatorFactory, IDataProvider<TSourceType, TSource, TExpression> dataProvider)
			: base(new DocumentAdapter(), scanner, new DocumentKeywordTokenParser<TSourceType>(), expressionParser, generatorFactory, dataProvider)
		{
		}

		/// <summary>
		/// Creates a new merge provider.
		/// </summary>
		/// <param name="scanner">An object used to find tokens in the document.</param>
		/// <param name="tokenParser">The object to use to parse the tokens' text.</param>
		/// <param name="expressionParser">The object to use to parse the raw expression text.</param>
		/// <param name="generatorFactory">An object that can assign custom generators.</param>
		/// <param name="dataProvider">An object used to access source data while merging.</param>
		protected DocumentMergeProvider(ITemplateScanner<Document, DocumentToken<Node>> scanner, ITokenParser<TSourceType> tokenParser, IExpressionParser<TSourceType, TExpression> expressionParser, IGeneratorFactory<IGenerator<Document, Node, TSourceType, TSource, TExpression>> generatorFactory, IDataProvider<TSourceType, TSource, TExpression> dataProvider)
			: base(new DocumentAdapter(), scanner, tokenParser, expressionParser, generatorFactory, dataProvider)
		{
		}

		/// <summary>
		/// Creates a new merge provider.
		/// </summary>
		/// <param name="adapter">An object used to access and manipulate the document.</param>
		/// <param name="scanner">An object used to find tokens in the document.</param>
		/// <param name="expressionParser">The object to use to parse the raw expression text.</param>
		/// <param name="generatorFactory">An object that can assign custom generators.</param>
		/// <param name="dataProvider">An object used to access source data while merging.</param>
		protected DocumentMergeProvider(IDocumentAdapter<Document, Node> adapter, ITemplateScanner<Document, DocumentToken<Node>> scanner, IExpressionParser<TSourceType, TExpression> expressionParser, IGeneratorFactory<IGenerator<Document, Node, TSourceType, TSource, TExpression>> generatorFactory, IDataProvider<TSourceType, TSource, TExpression> dataProvider)
			: base(adapter, scanner, new DocumentKeywordTokenParser<TSourceType>(), expressionParser, generatorFactory, dataProvider)
		{
		}

		/// <summary>
		/// Creates a new merge provider.
		/// </summary>
		/// <param name="adapter">An object used to access and manipulate the document.</param>
		/// <param name="scanner">An object used to find tokens in the document.</param>
		/// <param name="tokenParser">The object to use to parse the tokens' text.</param>
		/// <param name="expressionParser">The object to use to parse the raw expression text.</param>
		/// <param name="generatorFactory">An object that can assign custom generators.</param>
		/// <param name="dataProvider">An object used to access source data while merging.</param>
		protected DocumentMergeProvider(IDocumentAdapter<Document, Node> adapter, ITemplateScanner<Document, DocumentToken<Node>> scanner, ITokenParser<TSourceType> tokenParser, IExpressionParser<TSourceType, TExpression> expressionParser, IGeneratorFactory<IGenerator<Document, Node, TSourceType, TSource, TExpression>> generatorFactory, IDataProvider<TSourceType, TSource, TExpression> dataProvider)
			: base(adapter, scanner, tokenParser, expressionParser, generatorFactory, dataProvider)
		{
		}

		/// <summary>
		/// Gets or sets a value indicating whether a field's containing paragraph
		/// should be removed if the field is the only thing remaining in the paragraph.
		/// </summary>
		public bool RemoveParentParagraphOfEmptyFields { get; set; }

		/// <summary>
		/// Gets a value indicating whether or not to automatically detect
		/// that a string value represents an image data URI.
		/// </summary>
		public bool DetectImageDataUri { get; set; }

		/// <summary>
		/// Gets a value indicating whether or not to automatically detect that a string
		/// value represents HTML, and if so inject it into the document as HTML.
		/// </summary>
		public bool DetectHtml { get; set; }

		/// <summary>
		/// If a field's text value contains new-line characters, then insert the text into
		/// multiple paragraphs, so long as the field tag is the only thing in it's paragraph.
		/// </summary>
		public bool ConvertNewLinesToParagraphs { get; set; }

		/// <summary>
		/// An event that is raised before an HTML string is converted to document nodes.
		/// </summary>
		public event BeforeHtmlNodesCreatedEventHandler BeforeHtmlNodesCreated;

		/// <summary>
		/// An event that is raised after an HTML string has been converted to document nodes,
		/// but before they have been inserted into the document.
		/// </summary>
		public event AfterHtmlNodesCreatedEventHandler<TSourceType, TSource, TExpression> AfterHtmlNodesCreated;

		/// <summary>
		/// Gets or sets a value indicating whether or not a special field reference should
		/// be used in place of quote characters in merge values.
		/// </summary>
		/// <remarks>
		/// NOTE: Injecting quote characters into a document during the merge process corrupts IF fields,
		/// since the syntax relies on balancing quotes. Use this option to replace quote characters with
		/// a quote reference field, which looks like a quote character but is ignored by fields.
		/// </remarks>
		public bool UseFieldForQuotes { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether document nodes that are created from an HTML string
		/// should be automatically formatted according to the formatting that was applied to the field.
		/// </summary>
		public bool AutoFormatNodesFromHtml { get; set; }

		/// <summary>
		/// Copy styles from a field to a set of merged nodes.
		/// </summary>
		protected virtual void CopyStylesFromField(Field<Document, Node, DocumentToken<Node>, TSourceType, TSource, TExpression> field, Node mergedNode)
		{
			CopyStylesFromField(field, new[] { mergedNode });
		}

		/// <summary>
		/// Copy styles from a field to a set of merged nodes.
		/// </summary>
		protected virtual void CopyStylesFromField(Field<Document, Node, DocumentToken<Node>, TSourceType, TSource, TExpression> field, IEnumerable<Node> mergedNodes)
		{
			var fieldParagraph = (Paragraph)field.Token.Start.GetAncestor(NodeType.Paragraph);
			var fieldInline = field.Token.Start.GetSelfAndFollowingSiblings().TakeUpToNode(field.Token.End).OfType<Inline>().FirstOrDefault();

			foreach (var targetNode in mergedNodes)
			{
				// Copy field's paragraph formatting settings to target paragraph(s).
				if (fieldParagraph != null)
				{
					var mergedParagraph = targetNode as Paragraph;
					if (mergedParagraph != null)
						fieldParagraph.ParagraphFormat.CopyTo(mergedParagraph.ParagraphFormat);
				}

				// Copy field's first inline element's font settings to target inline(s).
				if (fieldInline != null)
				{
					var mergedInline = targetNode as Inline;
					if (mergedInline != null)
						fieldInline.Font.CopyTo(mergedInline.Font);

					var mergedComposite = targetNode as CompositeNode;
					if (mergedComposite != null)
					{
						foreach (var mergedChildInline in mergedComposite.ChildNodes.OfType<Inline>())
							fieldInline.Font.CopyTo(mergedChildInline.Font);
					}
				}
			}
		}

		/// <summary>
		/// Convert the given image data into an image shape.
		/// </summary>
		protected virtual Shape CreateImageFromData(Document document, Field<Document, Node, DocumentToken<Node>, TSourceType, TSource, TExpression> field, byte[] data)
		{
			var shape = new Shape(document, ShapeType.Image)
			{
				WrapType = WrapType.Inline
			};

			using (var stream = new MemoryStream(data))
			{
				shape.ImageData.SetImage(Image.FromStream(stream, true, true));
			}

			shape.Width = shape.ImageData.ImageSize.WidthPoints;
			shape.Height = shape.ImageData.ImageSize.HeightPoints;

			return shape;
		}

		/// <summary>
		/// Generate nodes for the given value to replace the given field.
		/// </summary>
		protected override Node[] GenerateStandardFieldContent(Document document, Field<Document, Node, DocumentToken<Node>, TSourceType, TSource, TExpression> field, string textValue)
		{
			var nodeList = new List<Node>();

			if (DetectImageDataUri && Regex.IsMatch(textValue, DataUriExpr))
			{
				var dataUriMatch = Regex.Match(textValue, DataUriExpr);

				var content = dataUriMatch.Groups["content"].Value.Trim();
				var data = Convert.FromBase64String(content);

				var image = CreateImageFromData(document, field, data);

				return new Node[] { image };
			}

			string html = null;

			// Automatically detect when the value that is being inserted looks like HTML markup,
			// and if so then convert the markup into the appropriate document nodes.
			if (DetectHtml && Regex.IsMatch(textValue, HtmlTextExpr, RegexOptions.IgnoreCase | RegexOptions.Multiline))
			{
				// NOTE: This class could provide a way for a more knowledgable party to identify markup.
				//       This could be useful in the event that some content is mis-identified as markup,
				//       or if for some other reason the markup should be embedded as raw value.

				html = textValue;
			}

			if (html != null)
			{
				if (BeforeHtmlNodesCreated != null)
				{
					// Raise the event and allow subscribers to manipulate the src value.
					var args = new BeforeHtmlNodesCreatedEventArgs
					{
						Html = html
					};

					BeforeHtmlNodesCreated(this, args);

					if (args.HtmlUpdated)
						html = args.Html;
				}

				// Use the adapter to convert the given 
				var nodesFromHtml = NodeGenerator.CreateNodesForHtml(document, html);
				nodeList.AddRange(nodesFromHtml);
			}
			else
			{
				string runText;

				// Encode the inserted text if necessary.

				switch (field.Token.Encoding)
				{
					case DocumentTextEncoding.Uri:
						runText = Uri.EscapeDataString(textValue);
						break;
					default:
						runText = textValue;
						break;
				}

				// If the value is not HTML markup, simply inject it as a run.
				var run = Adapter.CreateTextRun(document, runText);

				// Apply formatting from the field's first inline.
				CopyStylesFromField(field, run);

				nodeList.Add(run);
			}

			if (UseFieldForQuotes)
			{
				// Look for quotes in the merged content and convert them to references to a quote reference
				// field (while also ensuring that said reference field exists in the document). This may be
				// necessary because the merged field could be contained within a Word IF field, and so the
				// newly introduced quotes would interfere with the parsing of the IF field syntax.

				for (var i = 0; i < nodeList.Count; i++)
				{
					var node = nodeList[i];

					Node[] resultingNodes;
					if (QuoteCharacters.ConvertToFieldReferences(document, node, out resultingNodes))
					{
						// Remove the original node and add the new set of nodes at the same index. In practice, the
						// original node is likely still in the list, but there is no reason to assume that it is.
						nodeList.RemoveAt(i);
						nodeList.InsertRange(i, resultingNodes);

						// Advance the index in the list such that it currently rests at the last of the resulting nodes
						// (since they have already been addressed by the call to 'ConvertToSpecialQuotes'). Iteration
						// will then resume at what was the next item in the list (if there is one).
						i += resultingNodes.Length - 1;
					}
				}
			}

			if (html != null)
			{
				if (AutoFormatNodesFromHtml)
				{
					// Apply basic paragraph styles from the paragraph that the field was contained within,
					// and apply inline styles from the first inline element of the field.

					CopyStylesFromField(field, nodeList);
				}

				if (AfterHtmlNodesCreated != null)
				{
					var args = new AfterHtmlNodesCreatedEventArgs<TSourceType, TSource, TExpression>
					{
						Field = field,
						Nodes = nodeList
					};

					AfterHtmlNodesCreated(this, args);

					return args.Nodes.ToArray();
				}
			}

			return nodeList.ToArray();
		}

		/// <summary>
		/// Merge a standard field with the given value.
		/// </summary>
		protected override void MergeStandardFieldValue(Document document, Field<Document, Node, DocumentToken<Node>, TSourceType, TSource, TExpression> field, string textValue)
		{
			if (string.IsNullOrEmpty(textValue))
			{
				if (RemoveParentParagraphOfEmptyFields && field.Token.Start.ParentNode is Paragraph && field.Token.IsAloneInParent())
				{
					// Remove the parent paragraph, since the field was the only thing in the paragraph.
					Adapter.Remove(field.Token.Start.ParentNode);
				}
				else
				{
					// Blank values cause Aspose.Words 2.0.50727 to crash when generating a PDF, so replace empty values with a space.
					Writer.ReplaceToken(document, field.Token, GenerateStandardFieldContent(document, field, " "));
				}

				return;
			}

			if (ConvertNewLinesToParagraphs && textValue.IndexOfAny(new[] { '\r', '\n' }) >= 0 && field.Token.Start.ParentNode is Paragraph && field.Token.IsAloneInParent())
			{
				Run templateRun = null;
				var templatePara = ((Paragraph)field.Token.Start.ParentNode);

				// Make note of the spacing around the template paragraph, so that
				// we can ensure that the spacing before is applied to the first
				// paragraph and the spacing after is applied to the last paragraph.

				var spacingBefore = !templatePara.ParagraphFormat.SpaceBeforeAuto ? templatePara.ParagraphFormat.SpaceBefore : (double?)null;

				var spacingAfter = !templatePara.ParagraphFormat.SpaceAfterAuto ? templatePara.ParagraphFormat.SpaceAfter : (double?)null;

				Paragraph priorPara = null;

				var hasUsedTemplatePara = false;

				foreach (var line in textValue.Split('\n'))
				{
					// Convert blank lines to empty paragraphs.
					if (string.IsNullOrEmpty(line))
					{
						var blankPara = (Paragraph)templatePara.Clone(false);

						// Remove unwanted space between paragraphs.
						if (priorPara != null)
							blankPara.ParagraphFormat.SpaceBefore = 0;
						else
						{
							if (spacingBefore.HasValue)
								blankPara.ParagraphFormat.SpaceBefore = spacingBefore.Value;
							else
								blankPara.ParagraphFormat.SpaceBeforeAuto = true;
						}

						// Remove unwanted space between paragraphs.
						blankPara.ParagraphFormat.SpaceAfter = 0;

						if (priorPara != null)
							// Insert after the current paragraph.
							priorPara.ParentNode.InsertAfter(blankPara, priorPara);
						else
						{
							// Insert before the template paragraph.
							templatePara.ParentNode.InsertBefore(blankPara, templatePara);

							// Remove spacing before the template paragraph, since it is not the first paragraph.
							templatePara.ParagraphFormat.SpaceBefore = 0;
						}

						priorPara = blankPara;
					}
					else
					{
						Run run;
						Paragraph para;

						if (!hasUsedTemplatePara)
						{
							// Reuse the template paragraph for the first block.

							run = new Run(document, line);

							CopyStylesFromField(field, run);

							para = templatePara;

							// In the same way that the template paragraph is used for cloning,
							// use the run with styles copied from the template as well.
							hasUsedTemplatePara = true;
							templateRun = run;

							Writer.ReplaceToken(document, field.Token, run);
						}
						else
						{
							// Clone the run with styles copied from the template token.
							run = (Run)templateRun.Clone(false);

							para = (Paragraph)priorPara.Clone(false);

							run.Text = line;

							priorPara.ParentNode.InsertAfter(para, priorPara);
							para.AppendChild(run);
						}

						// Remove unwanted space between paragraphs.
						if (priorPara != null)
							para.ParagraphFormat.SpaceBefore = 0;
						else
						{
							if (spacingBefore.HasValue)
								para.ParagraphFormat.SpaceBefore = spacingBefore.Value;
							else
								para.ParagraphFormat.SpaceBeforeAuto = true;
						}

						// Remove unwanted space between paragraphs.
						para.ParagraphFormat.SpaceAfter = 0;

						priorPara = para;
					}
				}

				if (!hasUsedTemplatePara)
				{
					// The content was just whitespace, so remove the
					// original template paragraph, since it wasn't used.
					templatePara.Remove();
				}
				else
				{
					// Set after paragraphs spacing of the last paragraph
					if (spacingAfter.HasValue)
						priorPara.ParagraphFormat.SpaceAfter = spacingAfter.Value;
					else
						priorPara.ParagraphFormat.SpaceAfterAuto = true;
				}

				return;
			}

			base.MergeStandardFieldValue(document, field, textValue);
		}
	}

	/// <summary>
	/// An object which can be used to produce a merged document from a given document template and data context, using the text of nodes in the document.
	/// </summary>
	public class DocumentTextMergeProvider<TSourceType, TSource, TExpression> : DocumentMergeProvider<TSourceType, TSource, TExpression>
		where TSourceType : class
		where TExpression : class
	{
		/// <summary>
		/// Creates a new merge provider.
		/// </summary>
		/// <param name="tokenStart">The text that marks the start of a token.</param>
		/// <param name="tokenEnd">The text that marks the end of a token.</param>
		/// <param name="expressionParser">The object to use to parse the raw expression text.</param>
		/// <param name="generatorFactory">An object that can assign custom generators.</param>
		/// <param name="dataProvider">An object used to access source data while merging.</param>
		public DocumentTextMergeProvider(string tokenStart, string tokenEnd, IExpressionParser<TSourceType, TExpression> expressionParser, IGeneratorFactory<IGenerator<Document, Node, TSourceType, TSource, TExpression>> generatorFactory, IDataProvider<TSourceType, TSource, TExpression> dataProvider)
			: this(tokenStart, tokenEnd, '\0', false, new DocumentKeywordTokenParser<TSourceType>(), expressionParser, generatorFactory, dataProvider)
		{
		}

		/// <summary>
		/// Creates a new merge provider.
		/// </summary>
		/// <param name="tokenStart">The text that marks the start of a token.</param>
		/// <param name="tokenEnd">The text that marks the end of a token.</param>
		/// <param name="tokenParser">The object to use to parse the tokens' text.</param>
		/// <param name="expressionParser">The object to use to parse the raw expression text.</param>
		/// <param name="generatorFactory">An object that can assign custom generators.</param>
		/// <param name="dataProvider">An object used to access source data while merging.</param>
		public DocumentTextMergeProvider(string tokenStart, string tokenEnd, ITokenParser<TSourceType> tokenParser, IExpressionParser<TSourceType, TExpression> expressionParser, IGeneratorFactory<IGenerator<Document, Node, TSourceType, TSource, TExpression>> generatorFactory, IDataProvider<TSourceType, TSource, TExpression> dataProvider)
			: this(tokenStart, tokenEnd, '\0', false, tokenParser, expressionParser, generatorFactory, dataProvider)
		{
		}

		/// <summary>
		/// Creates a new merge provider.
		/// </summary>
		/// <param name="tokenStart">The text that marks the start of a token.</param>
		/// <param name="tokenEnd">The text that marks the end of a token.</param>
		/// <param name="tokenEscapeCharacter">The character used to escape characters which should not be considered a start or end marker of a token.</param>
		/// <param name="expressionParser">The object to use to parse the raw expression text.</param>
		/// <param name="generatorFactory">An object that can assign custom generators.</param>
		/// <param name="dataProvider">An object used to access source data while merging.</param>
		public DocumentTextMergeProvider(string tokenStart, string tokenEnd, char tokenEscapeCharacter, IExpressionParser<TSourceType, TExpression> expressionParser, IGeneratorFactory<IGenerator<Document, Node, TSourceType, TSource, TExpression>> generatorFactory, IDataProvider<TSourceType, TSource, TExpression> dataProvider)
			: this(new DocumentAdapter(), tokenStart, tokenEnd, tokenEscapeCharacter, false, new DocumentKeywordTokenParser<TSourceType>(), expressionParser, generatorFactory, dataProvider)
		{
		}

		/// <summary>
		/// Creates a new merge provider.
		/// </summary>
		/// <param name="tokenStart">The text that marks the start of a token.</param>
		/// <param name="tokenEnd">The text that marks the end of a token.</param>
		/// <param name="tokenEscapeCharacter">The character used to escape characters which should not be considered a start or end marker of a token.</param>
		/// <param name="tokenStrictMode">If true, nested or unexpected token characters are not allowed and will result in an exception.</param>
		/// <param name="expressionParser">The object to use to parse the raw expression text.</param>
		/// <param name="generatorFactory">An object that can assign custom generators.</param>
		/// <param name="dataProvider">An object used to access source data while merging.</param>
		public DocumentTextMergeProvider(string tokenStart, string tokenEnd, char tokenEscapeCharacter, bool tokenStrictMode, IExpressionParser<TSourceType, TExpression> expressionParser, IGeneratorFactory<IGenerator<Document, Node, TSourceType, TSource, TExpression>> generatorFactory, IDataProvider<TSourceType, TSource, TExpression> dataProvider)
			: this(new DocumentAdapter(), tokenStart, tokenEnd, tokenEscapeCharacter, tokenStrictMode, new DocumentKeywordTokenParser<TSourceType>(), expressionParser, generatorFactory, dataProvider)
		{
		}

		/// <summary>
		/// Creates a new merge provider.
		/// </summary>
		/// <param name="tokenStart">The text that marks the start of a token.</param>
		/// <param name="tokenEnd">The text that marks the end of a token.</param>
		/// <param name="tokenEscapeCharacter">The character used to escape characters which should not be considered a start or end marker of a token.</param>
		/// <param name="tokenParser">The object to use to parse the tokens' text.</param>
		/// <param name="expressionParser">The object to use to parse the raw expression text.</param>
		/// <param name="generatorFactory">An object that can assign custom generators.</param>
		/// <param name="dataProvider">An object used to access source data while merging.</param>
		public DocumentTextMergeProvider(string tokenStart, string tokenEnd, char tokenEscapeCharacter, ITokenParser<TSourceType> tokenParser, IExpressionParser<TSourceType, TExpression> expressionParser,
			IGeneratorFactory<IGenerator<Document, Node, TSourceType, TSource, TExpression>> generatorFactory, IDataProvider<TSourceType, TSource, TExpression> dataProvider)
			: this(new DocumentAdapter(), tokenStart, tokenEnd, tokenEscapeCharacter, false, tokenParser, expressionParser, generatorFactory, dataProvider)
		{
		}

		/// <summary>
		/// Creates a new merge provider.
		/// </summary>
		/// <param name="tokenStart">The text that marks the start of a token.</param>
		/// <param name="tokenEnd">The text that marks the end of a token.</param>
		/// <param name="tokenEscapeCharacter">The character used to escape characters which should not be considered a start or end marker of a token.</param>
		/// <param name="tokenStrictMode">If true, nested or unexpected token characters are not allowed and will result in an exception.</param>
		/// <param name="tokenParser">The object to use to parse the tokens' text.</param>
		/// <param name="expressionParser">The object to use to parse the raw expression text.</param>
		/// <param name="generatorFactory">An object that can assign custom generators.</param>
		/// <param name="dataProvider">An object used to access source data while merging.</param>
		public DocumentTextMergeProvider(string tokenStart, string tokenEnd, char tokenEscapeCharacter, bool tokenStrictMode, ITokenParser<TSourceType> tokenParser, IExpressionParser<TSourceType, TExpression> expressionParser, IGeneratorFactory<IGenerator<Document, Node, TSourceType, TSource, TExpression>> generatorFactory, IDataProvider<TSourceType, TSource, TExpression> dataProvider)
			: this(new DocumentAdapter(), tokenStart, tokenEnd, tokenEscapeCharacter, tokenStrictMode, tokenParser, expressionParser, generatorFactory, dataProvider)
		{
		}

		/// <summary>
		/// Creates a new merge provider.
		/// </summary>
		/// <param name="adapter">An object used to access and manipulate the document.</param>
		/// <param name="tokenStart">The text that marks the start of a token.</param>
		/// <param name="tokenEnd">The text that marks the end of a token.</param>
		/// <param name="tokenEscapeCharacter">The character used to escape characters which should not be considered a start or end marker of a token.</param>
		/// <param name="tokenStrictMode">If true, nested or unexpected token characters are not allowed and will result in an exception.</param>
		/// <param name="tokenParser">The object to use to parse the tokens' text.</param>
		/// <param name="expressionParser">The object to use to parse the raw expression text.</param>
		/// <param name="generatorFactory">An object that can assign custom generators.</param>
		/// <param name="dataProvider">An object used to access source data while merging.</param>
		private DocumentTextMergeProvider(IDocumentAdapter<Document, Node> adapter, string tokenStart, string tokenEnd, char tokenEscapeCharacter, bool tokenStrictMode, ITokenParser<TSourceType> tokenParser, IExpressionParser<TSourceType, TExpression> expressionParser, IGeneratorFactory<IGenerator<Document, Node, TSourceType, TSource, TExpression>> generatorFactory, IDataProvider<TSourceType, TSource, TExpression> dataProvider)
			: base(adapter, new DocumentTextScanner(adapter, tokenStart, tokenEnd, tokenEscapeCharacter, tokenStrictMode), tokenParser, expressionParser, generatorFactory, dataProvider)
		{
		}
	}

	/// <summary>
	/// A delegate for the event that is raised before an HTML string is converted to document nodes.
	/// </summary>
	/// <param name="sender">The event sender.</param>
	/// <param name="args">The event arguments.</param>
	public delegate void BeforeHtmlNodesCreatedEventHandler(object sender, BeforeHtmlNodesCreatedEventArgs args);

	/// <summary>
	/// The event args for the event that is raised before an HTML string is converted to document nodes.
	/// Allows a subscriber to modify the HTML markup as needed.
	/// </summary>
	public class BeforeHtmlNodesCreatedEventArgs
	{
		/// <summary>
		/// Gets the HTML that will be converted into nodes.
		/// </summary>
		public string Html { get; internal set; }

		/// <summary>
		/// Gets a value indicating whether the HTML has been updated.
		/// </summary>
		internal bool HtmlUpdated { get; private set; }

		/// <summary>
		/// Updates the HTML that will be converted into nodes. 
		/// </summary>
		/// <param name="html">The new HTML to use.</param>
		public void Update(string html)
		{
			HtmlUpdated = true;
			Html = html;
		}
	}

	/// <summary>
	/// A delegate for the event that is raised after an HTML string has been converted
	/// to document nodes, but before they have been inserted into the document.
	/// </summary>
	/// <typeparam name="TSourceType">The type that represents the type of source data, e.g. 'Type'.</typeparam>
	/// <typeparam name="TSource">The type of the source data to merge, e.g. 'Object'.</typeparam>
	/// <typeparam name="TExpression">The type of parsed expressions.</typeparam>
	/// <param name="sender">The event sender.</param>
	/// <param name="args">The event arguments.</param>
	public delegate void AfterHtmlNodesCreatedEventHandler<TSourceType, TSource, TExpression>(object sender, AfterHtmlNodesCreatedEventArgs<TSourceType, TSource, TExpression> args)
		where TExpression : class;

	/// <summary>
	/// The event args for the event that is raised after an HTML string has been converted
	/// to document nodes, but before they have been inserted into the document. Allows a
	/// subscriber to manipulate the nodes before they are inserted into the document.
	/// </summary>
	/// <typeparam name="TSourceType">The type that represents the type of source data, e.g. 'Type'.</typeparam>
	/// <typeparam name="TSource">The type of the source data to merge, e.g. 'Object'.</typeparam>
	/// <typeparam name="TExpression">The type of parsed expressions.</typeparam>
	public class AfterHtmlNodesCreatedEventArgs<TSourceType, TSource, TExpression>
		where TExpression : class
	{
		/// <summary>
		/// Gets the field that will be replaced.
		/// </summary>
		public Field<Document, Node, DocumentToken<Node>, TSourceType, TSource, TExpression> Field { get; internal set; }

		/// <summary>
		/// Gets the nodes that will be inserted into the document.
		/// </summary>
		public List<Node> Nodes { get; internal set; }
	}
}
