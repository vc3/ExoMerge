using Aspose.Words;

namespace ExoMerge.Aspose.Extensions
{
	public static class ParagraphFormatExtensions
	{
		/// <summary>
		/// Copy the pargraph format's settings into the target paragraph format.
		/// </summary>
		/// <param name="format">The paragraph format to copy settings from.</param>
		/// <param name="target">The paragraph format to copy settings to.</param>
		public static void CopyTo(this ParagraphFormat format, ParagraphFormat target)
		{
			if (ReferenceEquals(format, target))
				return;

			target.Alignment = format.Alignment;
			target.Bidi = format.Bidi;
			target.FirstLineIndent = format.FirstLineIndent;
			target.KeepTogether = format.KeepTogether;
			target.KeepWithNext = format.KeepWithNext;
			target.LeftIndent = format.LeftIndent;
			target.LineSpacing = format.LineSpacing;
			target.LineSpacingRule = format.LineSpacingRule;
			target.NoSpaceBetweenParagraphsOfSameStyle = format.NoSpaceBetweenParagraphsOfSameStyle;
			target.OutlineLevel = format.OutlineLevel;
			target.PageBreakBefore = format.PageBreakBefore;
			target.RightIndent = format.RightIndent;
			target.SpaceAfter = format.SpaceAfter;
			target.SpaceAfterAuto = format.SpaceAfterAuto;
			target.SpaceBefore = format.SpaceBefore;
			target.SpaceBeforeAuto = format.SpaceBeforeAuto;

			var style = target.Style.Styles[format.Style.Name];
			if (style != null)
				target.Style = style;

			target.SuppressAutoHyphens = format.SuppressAutoHyphens;
			target.WidowControl = format.WidowControl;
		}
	}
}
