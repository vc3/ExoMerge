using System.Collections.Generic;
using System.Globalization;
using Aspose.Words;
using ExoMerge.Rendering;

namespace ExoMerge.Aspose.Common
{
	/// <summary>
	/// Generates a Wingdings checked or unchecked box, depending on whether the expression evalutes to a "truthy" value.
	/// </summary>
	/// <typeparam name="TSourceType">The type that identifies the type of the data source, e.g. 'Type'.</typeparam>
	/// <typeparam name="TSource">The type of the source data to merge, e.g. 'Object'.</typeparam>
	/// <typeparam name="TExpression">The type of parsed expressions.</typeparam>
	public class CheckBoxSymbolGenerator<TSourceType, TSource, TExpression> : BooleanGenerator<Document, Node, TSourceType, TSource, TExpression>
		where TExpression : class
	{
		private const char WingdingsUnchecked = (char)168;

		private const char WingdingsChecked = (char)254;

		protected override IEnumerable<Node> GenerateContent(Document document, bool isChecked)
		{
			var checkboxCharacter = isChecked ? WingdingsChecked : WingdingsUnchecked;

			var run = new Run(document, checkboxCharacter.ToString(CultureInfo.InvariantCulture));

			run.Font.Name = "Wingdings";

			yield return run;
		}
	}
}
